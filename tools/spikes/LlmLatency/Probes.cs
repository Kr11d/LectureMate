using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Anthropic;
using Anthropic.Models.Messages;
using OpenAI.Chat;

namespace LlmLatency;

/// <summary>
/// The same shape as <c>LlmRequest</c> in Core: system, cached prefix, variable prompt.
/// </summary>
internal sealed record ProbeRequest(string System, string CachedPrefix, string Prompt, int MaxTokens);

/// <summary>
/// Timings and token usage of one streamed request.
/// </summary>
/// <param name="TimeToFirstToken">Time until the first non-empty text fragment; null if no text arrived.</param>
/// <param name="InputTokens">All prompt tokens, cached ones included.</param>
/// <param name="CacheWriteTokens">Tokens written to the cache; null if the provider does not report it.</param>
internal sealed record ProbeResult(
    TimeSpan? TimeToFirstToken,
    TimeSpan Total,
    long InputTokens,
    long CachedTokens,
    long? CacheWriteTokens,
    long OutputTokens,
    string Answer);

internal interface IProviderProbe
{
    string Provider { get; }

    string Model { get; }

    Task<ProbeResult> SendAsync(ProbeRequest request, CancellationToken ct);
}

internal sealed class OpenAiProbe(string apiKey, string model, string reasoningEffort) : IProviderProbe
{
    private readonly ChatClient _client = new(model, apiKey);

    public string Provider => "OpenAI";

    public string Model => model;

    public async Task<ProbeResult> SendAsync(ProbeRequest request, CancellationToken ct)
    {
        // OpenAI caches the longest matching prefix automatically (>= 1024 tokens), so no marker is needed.
        List<ChatMessage> messages =
        [
            new SystemChatMessage(request.System),
            new UserChatMessage(
                ChatMessageContentPart.CreateTextPart(request.CachedPrefix),
                ChatMessageContentPart.CreateTextPart(request.Prompt)),
        ];
        // The SDK marks reasoning effort as experimental (OPENAI001), but without "none"
        // the model reasons before answering and the time to first token is not comparable.
#pragma warning disable OPENAI001
        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = request.MaxTokens,
            ReasoningEffortLevel = new ChatReasoningEffortLevel(reasoningEffort),
        };
#pragma warning restore OPENAI001

        var answer = new StringBuilder();
        TimeSpan? firstToken = null;
        long input = 0, cached = 0, output = 0;

        var start = Stopwatch.GetTimestamp();
        await foreach (var update in _client.CompleteChatStreamingAsync(messages, options, ct))
        {
            foreach (var part in update.ContentUpdate)
            {
                if (!string.IsNullOrEmpty(part.Text))
                {
                    firstToken ??= Stopwatch.GetElapsedTime(start);
                    answer.Append(part.Text);
                }
            }

            if (update.Usage is { } usage)
            {
                input = usage.InputTokenCount;
                output = usage.OutputTokenCount;
                cached = usage.InputTokenDetails?.CachedTokenCount ?? 0;
            }
        }

        return new ProbeResult(firstToken, Stopwatch.GetElapsedTime(start), input, cached, null, output, answer.ToString());
    }
}

internal sealed class AnthropicProbe(string apiKey, string model, bool adaptiveThinking) : IProviderProbe
{
    private readonly AnthropicClient _client = new() { ApiKey = apiKey };

    public string Provider => "Anthropic";

    public string Model => model;

    public async Task<ProbeResult> SendAsync(ProbeRequest request, CancellationToken ct)
    {
        ThinkingConfigParam thinking = adaptiveThinking ? new ThinkingConfigAdaptive() : new ThinkingConfigDisabled();
        var parameters = new MessageCreateParams
        {
            Model = model,
            MaxTokens = request.MaxTokens,
            System = new List<TextBlockParam> { new() { Text = request.System } },
            Thinking = thinking,
            Messages =
            [
                new()
                {
                    Role = Role.User,
                    Content = new List<ContentBlockParam>
                    {
                        // Cache boundary: system + materials up to this block form the cached prefix.
                        new TextBlockParam { Text = request.CachedPrefix, CacheControl = new CacheControlEphemeral() },
                        new TextBlockParam { Text = request.Prompt },
                    },
                },
            ],
        };

        var answer = new StringBuilder();
        TimeSpan? firstToken = null;
        long input = 0, cacheRead = 0, cacheWrite = 0, output = 0;

        var start = Stopwatch.GetTimestamp();
        await foreach (var streamEvent in _client.Messages.CreateStreaming(parameters, ct))
        {
            if (streamEvent.TryPickStart(out var started))
            {
                var usage = started.Message.Usage;
                input = usage.InputTokens;
                cacheRead = usage.CacheReadInputTokens ?? 0;
                cacheWrite = usage.CacheCreationInputTokens ?? 0;
            }
            else if (streamEvent.TryPickContentBlockDelta(out var delta)
                && delta.Delta.TryPickText(out var text)
                && text.Text.Length > 0)
            {
                firstToken ??= Stopwatch.GetElapsedTime(start);
                answer.Append(text.Text);
            }
            else if (streamEvent.TryPickDelta(out var messageDelta))
            {
                output = messageDelta.Usage.OutputTokens;
            }
        }

        // Anthropic reports uncached, cache-read and cache-write tokens separately.
        var totalInput = input + cacheRead + cacheWrite;
        return new ProbeResult(firstToken, Stopwatch.GetElapsedTime(start), totalInput, cacheRead, cacheWrite, output, answer.ToString());
    }
}

/// <summary>
/// Gemini API over plain HTTP (streamGenerateContent with SSE), so the spike needs no extra package.
/// </summary>
internal sealed class GeminiProbe(string apiKey, string model, string thinkingLevel) : IProviderProbe
{
    private readonly HttpClient _http = new() { BaseAddress = new Uri("https://generativelanguage.googleapis.com/v1beta/") };

    public string Provider => "Gemini";

    public string Model => model;

    public async Task<ProbeResult> SendAsync(ProbeRequest request, CancellationToken ct)
    {
        // Gemini caches repeated prefixes implicitly (no marker); hits show up as cachedContentTokenCount.
        var body = new JsonObject
        {
            ["systemInstruction"] = new JsonObject { ["parts"] = new JsonArray(new JsonObject { ["text"] = request.System }) },
            ["contents"] = new JsonArray(new JsonObject
            {
                ["role"] = "user",
                ["parts"] = new JsonArray(
                    new JsonObject { ["text"] = request.CachedPrefix },
                    new JsonObject { ["text"] = request.Prompt }),
            }),
            ["generationConfig"] = new JsonObject
            {
                ["maxOutputTokens"] = request.MaxTokens,
                ["thinkingConfig"] = new JsonObject { ["thinkingLevel"] = thinkingLevel },
            },
        };

        // The key goes in a header, not in the URL, so it never lands in proxy or HTTP logs.
        using var message = new HttpRequestMessage(HttpMethod.Post, $"models/{model}:streamGenerateContent?alt=sse")
        {
            Content = JsonContent.Create(body),
        };
        message.Headers.Add("x-goog-api-key", apiKey);

        var answer = new StringBuilder();
        TimeSpan? firstToken = null;
        long input = 0, cached = 0, output = 0;

        var start = Stopwatch.GetTimestamp();
        using var response = await _http.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"{(int)response.StatusCode}: {error[..Math.Min(error.Length, 300)]}", null, response.StatusCode);
        }

        using var reader = new StreamReader(await response.Content.ReadAsStreamAsync(ct));
        while (await reader.ReadLineAsync(ct) is { } line)
        {
            if (!line.StartsWith("data: ", StringComparison.Ordinal))
            {
                continue;
            }

            using var chunk = JsonDocument.Parse(line.AsMemory(6));
            var root = chunk.RootElement;
            if (root.TryGetProperty("candidates", out var candidates)
                && candidates[0].TryGetProperty("content", out var content)
                && content.TryGetProperty("parts", out var parts))
            {
                foreach (var part in parts.EnumerateArray())
                {
                    // Thought summaries are not answer text, so they do not count as the first token.
                    var isThought = part.TryGetProperty("thought", out var thought) && thought.GetBoolean();
                    if (!isThought && part.TryGetProperty("text", out var text) && text.GetString() is { Length: > 0 } fragment)
                    {
                        firstToken ??= Stopwatch.GetElapsedTime(start);
                        answer.Append(fragment);
                    }
                }
            }

            if (root.TryGetProperty("usageMetadata", out var usage))
            {
                input = Count(usage, "promptTokenCount");
                cached = Count(usage, "cachedContentTokenCount");
                output = Count(usage, "candidatesTokenCount") + Count(usage, "thoughtsTokenCount");
            }
        }

        return new ProbeResult(firstToken, Stopwatch.GetElapsedTime(start), input, cached, null, output, answer.ToString());

        static long Count(JsonElement usage, string name) =>
            usage.TryGetProperty(name, out var value) ? value.GetInt64() : 0;
    }
}
