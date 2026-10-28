using System.Globalization;
using System.Text;
using LlmLatency;

Console.OutputEncoding = Encoding.UTF8;

var options = SpikeOptions.Parse(args);
if (options is null)
{
    Console.WriteLine(SpikeOptions.Usage);
    return 1;
}

var projectDir = FindProjectDir();
var materials = Scenario.LoadMaterials(options.MaterialsPath ?? Path.Combine(projectDir, "data"));
Console.WriteLine($"System prompt: {Scenario.SystemPrompt.Length} chars; materials: {materials.Length} chars "
    + $"(rough estimate {materials.Length / 3} tokens, real counts come from usage).");

if (options.DryRun)
{
    var prefix = Scenario.BuildCachedPrefix(materials, Guid.NewGuid());
    var prompt = Scenario.BuildPrompt(Scenario.Questions[0]);
    Console.WriteLine($"""

        Request layout: system -> cached prefix (cache boundary) -> prompt
        --- system ---
        {Scenario.SystemPrompt}
        --- cached prefix: first 300 of {prefix.Length} chars ---
        {prefix[..300]}
        --- prompt ({prompt.Length} chars) ---
        {prompt}
        """);
    return 0;
}

var probes = new List<IProviderProbe>();
if (options.RunOpenAi)
{
    AddProbe("OPENAI_API_KEY", key => new OpenAiProbe(key, options.OpenAiModel, options.OpenAiReasoningEffort));
}

if (options.RunAnthropic)
{
    AddProbe("ANTHROPIC_API_KEY", key => new AnthropicProbe(key, options.AnthropicModel, options.AnthropicAdaptiveThinking));
}

if (probes.Count == 0)
{
    Console.WriteLine("No provider to measure: set OPENAI_API_KEY and/or ANTHROPIC_API_KEY.");
    return 1;
}

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

// Open the HTTPS connection first, so the cold request measures the prompt cache, not the TLS handshake.
foreach (var probe in probes)
{
    try
    {
        await probe.SendAsync(new ProbeRequest("Відповідай одним словом.", "Привіт.", "Як справи?", 16), cts.Token);
        Console.WriteLine($"[{probe.Provider,-9}] connection warmed up ({probe.Model})");
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
        Console.WriteLine($"[{probe.Provider,-9}] warm-up failed: {Describe(ex)}");
    }
}

var measurements = new List<Measurement>();
var questionIndex = 0;
try
{
    // Providers alternate run by run, so a network hiccup affects both rather than one.
    for (var run = 1; run <= options.Runs; run++)
    {
        foreach (var probe in probes)
        {
            var prefix = Scenario.BuildCachedPrefix(materials, Guid.NewGuid());
            for (var i = 0; i <= options.WarmPerRun; i++)
            {
                var question = Scenario.Questions[questionIndex++ % Scenario.Questions.Count];
                var request = new ProbeRequest(Scenario.SystemPrompt, prefix, Scenario.BuildPrompt(question), options.MaxTokens);
                var phase = i == 0 ? Phase.Cold : Phase.Warm;
                Measurement measurement;
                try
                {
                    var result = await probe.SendAsync(request, cts.Token);
                    measurement = new Measurement(probe.Provider, probe.Model, run, phase, question, result, null);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    measurement = new Measurement(probe.Provider, probe.Model, run, phase, question, null, Describe(ex));
                }

                measurements.Add(measurement);
                Console.WriteLine(Report.FormatLine(measurement));
            }
        }
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Cancelled; reporting what was measured so far.");
}

Console.WriteLine();
Console.WriteLine(Report.FormatSummary(measurements));

var outDir = Directory.CreateDirectory(Path.Combine(projectDir, "out")).FullName;
var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
Report.WriteCsv(Path.Combine(outDir, $"results-{stamp}.csv"), measurements);
Report.WriteAnswers(Path.Combine(outDir, $"answers-{stamp}.md"), measurements);
Console.WriteLine($"Saved results-{stamp}.csv and answers-{stamp}.md to {outDir} (git-ignored).");
return 0;

void AddProbe(string keyVariable, Func<string, IProviderProbe> create)
{
    var key = Environment.GetEnvironmentVariable(keyVariable);
    if (string.IsNullOrWhiteSpace(key))
    {
        Console.WriteLine($"{keyVariable} is not set, skipping this provider.");
        return;
    }

    probes.Add(create(key));
}

// Exception messages from the SDKs carry status codes and API error text, never the key.
static string Describe(Exception ex) => $"{ex.GetType().Name}: {ex.Message}";

static string FindProjectDir()
{
    for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
    {
        if (File.Exists(Path.Combine(dir.FullName, "LlmLatency.csproj")))
        {
            return dir.FullName;
        }
    }

    return Directory.GetCurrentDirectory();
}
