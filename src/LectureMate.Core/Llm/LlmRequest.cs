namespace LectureMate.Core.Llm;

/// <summary>
/// A provider-neutral request to a language model.
/// </summary>
/// <param name="System">System instructions for the model.</param>
/// <param name="CachedContext">
/// The stable prefix: course materials fixed for the whole session. It must stay
/// byte-for-byte identical between requests so the provider can cache it.
/// </param>
/// <param name="Prompt">
/// The variable part after the prefix: recent transcript, the question and an optional screenshot.
/// </param>
/// <param name="MaxTokens">Upper limit on generated tokens.</param>
/// <remarks>
/// Parts are sent in the order system, <paramref name="CachedContext"/>, <paramref name="Prompt"/>.
/// Clients whose provider needs an explicit cache marker put it after the last part of
/// <paramref name="CachedContext"/>.
/// </remarks>
public sealed record LlmRequest(
    string System,
    IReadOnlyList<MessagePart> CachedContext,
    IReadOnlyList<MessagePart> Prompt,
    int MaxTokens);
