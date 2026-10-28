using System.Globalization;

namespace LlmLatency;

internal sealed record SpikeOptions
{
    public bool RunOpenAi { get; init; } = true;

    public bool RunAnthropic { get; init; } = true;

    /// <summary>Runs per provider. Each run starts with a fresh prefix, so its first request is cold.</summary>
    public int Runs { get; init; } = 10;

    /// <summary>Requests per run after the cold one, reusing the same prefix.</summary>
    public int WarmPerRun { get; init; } = 2;

    public int MaxTokens { get; init; } = 400;

    public string OpenAiModel { get; init; } = "gpt-6-luna";

    public string OpenAiReasoningEffort { get; init; } = "none";

    public string AnthropicModel { get; init; } = "claude-haiku-5-5";

    public bool AnthropicAdaptiveThinking { get; init; }

    public string? MaterialsPath { get; init; }

    public bool DryRun { get; init; }

    public const string Usage = """
        Usage: dotnet run -- [options]
          --provider both|openai|anthropic   (default: both)
          --runs N                           runs per provider, each starts cold (default: 10)
          --warm N                           warm requests per run (default: 2)
          --max-tokens N                     (default: 400)
          --openai-model ID                  (default: gpt-6-luna)
          --openai-effort none|low|...       reasoning effort (default: none)
          --anthropic-model ID               (default: claude-haiku-5-5)
          --anthropic-thinking disabled|adaptive  (default: disabled)
          --materials PATH                   *.md folder or a single file (default: data/)
          --dry-run                          print the request layout, no network calls
        Keys are read only from OPENAI_API_KEY and ANTHROPIC_API_KEY.
        """;

    /// <summary>Parses command-line arguments; returns null when they are invalid or help is requested.</summary>
    public static SpikeOptions? Parse(string[] args)
    {
        var options = new SpikeOptions();
        for (var i = 0; i < args.Length; i++)
        {
            var name = args[i];
            if (name is "--dry-run")
            {
                options = options with { DryRun = true };
                continue;
            }

            if (name is "--help" or "-h" || i + 1 >= args.Length)
            {
                return null;
            }

            var value = args[++i];
            options = name switch
            {
                "--provider" => value switch
                {
                    "both" => options with { RunOpenAi = true, RunAnthropic = true },
                    "openai" => options with { RunOpenAi = true, RunAnthropic = false },
                    "anthropic" => options with { RunOpenAi = false, RunAnthropic = true },
                    _ => null,
                },
                "--runs" => ParsePositive(value) is { } runs ? options with { Runs = runs } : null,
                "--warm" => ParseNonNegative(value) is { } warm ? options with { WarmPerRun = warm } : null,
                "--max-tokens" => ParsePositive(value) is { } max ? options with { MaxTokens = max } : null,
                "--openai-model" => options with { OpenAiModel = value },
                "--openai-effort" => options with { OpenAiReasoningEffort = value },
                "--anthropic-model" => options with { AnthropicModel = value },
                "--anthropic-thinking" => value switch
                {
                    "disabled" => options with { AnthropicAdaptiveThinking = false },
                    "adaptive" => options with { AnthropicAdaptiveThinking = true },
                    _ => null,
                },
                "--materials" => options with { MaterialsPath = value },
                _ => null,
            };
            if (options is null)
            {
                return null;
            }
        }

        return options;
    }

    private static int? ParsePositive(string value) =>
        int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var n) && n > 0 ? n : null;

    private static int? ParseNonNegative(string value) =>
        int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var n) ? n : null;
}
