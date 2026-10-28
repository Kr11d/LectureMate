using System.Globalization;
using System.Text;

namespace LlmLatency;

internal enum Phase
{
    Cold,
    Warm,
}

internal sealed record Measurement(string Provider, string Model, int Run, Phase Phase, string Question, ProbeResult? Result, string? Error);

internal static class Report
{
    /// <summary>
    /// Percentile with linear interpolation between closest ranks (the numpy default).
    /// </summary>
    public static double Percentile(IReadOnlyList<double> values, double p)
    {
        if (values.Count == 0)
        {
            return double.NaN;
        }

        var sorted = values.Order().ToArray();
        var rank = p * (sorted.Length - 1);
        var lower = (int)Math.Floor(rank);
        var upper = (int)Math.Ceiling(rank);
        return sorted[lower] + ((sorted[upper] - sorted[lower]) * (rank - lower));
    }

    public static string FormatLine(Measurement m)
    {
        var head = $"[{m.Provider,-9}] run {m.Run,2} {m.Phase,-4}";
        if (m.Result is not { } r)
        {
            return $"{head}  ERROR {m.Error}";
        }

        var ttft = r.TimeToFirstToken is { } t ? $"{t.TotalMilliseconds,6:F0}" : "     -";
        var write = r.CacheWriteTokens is { } w ? w.ToString(CultureInfo.InvariantCulture) : "n/a";
        return $"{head}  ttft={ttft} ms  total={r.Total.TotalMilliseconds,6:F0} ms  "
            + $"in={r.InputTokens} cached={r.CachedTokens} write={write} out={r.OutputTokens}";
    }

    public static string FormatSummary(IReadOnlyList<Measurement> measurements)
    {
        var builder = new StringBuilder();
        builder.AppendLine("provider   model                  phase  ok/n   TTFT p50  TTFT p90  total p50  total p90  in tok  cache hits  NO_Q");
        var groups = measurements
            .GroupBy(m => (m.Provider, m.Model, m.Phase))
            .OrderBy(g => g.Key.Provider, StringComparer.Ordinal)
            .ThenBy(g => g.Key.Phase);
        foreach (var group in groups)
        {
            var ok = group.Where(m => m.Result is not null).Select(m => m.Result!).ToList();
            var ttft = ok.Where(r => r.TimeToFirstToken is not null).Select(r => r.TimeToFirstToken!.Value.TotalMilliseconds).ToList();
            var total = ok.Select(r => r.Total.TotalMilliseconds).ToList();
            var hits = ok.Count(r => r.CachedTokens > 0);
            var noQuestion = ok.Count(r => r.Answer.Trim() == Scenario.NoQuestionMarker);
            var input = ok.Count > 0 ? ok.Average(r => r.InputTokens) : 0;

            builder.AppendLine(CultureInfo.InvariantCulture, $"{group.Key.Provider,-10} {group.Key.Model,-22} {group.Key.Phase,-5}  {ok.Count,2}/{group.Count(),-3} "
                + $"{Ms(Percentile(ttft, 0.5))} {Ms(Percentile(ttft, 0.9))} {Ms(Percentile(total, 0.5)),9} {Ms(Percentile(total, 0.9)),9} "
                + $"{input,7:F0}  {hits,3}/{ok.Count,-6} {noQuestion,4}");
        }

        return builder.ToString();

        static string Ms(double value) => double.IsNaN(value) ? "        -" : $"{value,6:F0} ms";
    }

    /// <summary>Timings and token counts only, without answer texts.</summary>
    public static void WriteCsv(string path, IReadOnlyList<Measurement> measurements)
    {
        var builder = new StringBuilder("provider,model,run,phase,ttft_ms,total_ms,input_tokens,cached_tokens,cache_write_tokens,output_tokens,error\n");
        foreach (var m in measurements)
        {
            var r = m.Result;
            builder.AppendJoin(',',
                m.Provider,
                m.Model,
                m.Run.ToString(CultureInfo.InvariantCulture),
                m.Phase,
                r?.TimeToFirstToken?.TotalMilliseconds.ToString("F0", CultureInfo.InvariantCulture) ?? "",
                r?.Total.TotalMilliseconds.ToString("F0", CultureInfo.InvariantCulture) ?? "",
                r?.InputTokens.ToString(CultureInfo.InvariantCulture) ?? "",
                r?.CachedTokens.ToString(CultureInfo.InvariantCulture) ?? "",
                r?.CacheWriteTokens?.ToString(CultureInfo.InvariantCulture) ?? "",
                r?.OutputTokens.ToString(CultureInfo.InvariantCulture) ?? "",
                m.Error?.Replace(',', ';').ReplaceLineEndings(" ") ?? "");
            builder.Append('\n');
        }

        File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
    }

    /// <summary>Answers for judging quality by eye. Stays in the git-ignored out/ folder.</summary>
    public static void WriteAnswers(string path, IReadOnlyList<Measurement> measurements)
    {
        var builder = new StringBuilder();
        foreach (var m in measurements.Where(m => m.Result is not null))
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"## {m.Provider} · {m.Model} · run {m.Run} · {m.Phase}");
            builder.AppendLine().AppendLine($"**Питання:** {m.Question}").AppendLine();
            builder.AppendLine(m.Result!.Answer).AppendLine();
        }

        File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
    }
}
