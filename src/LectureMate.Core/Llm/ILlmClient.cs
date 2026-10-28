namespace LectureMate.Core.Llm;

/// <summary>
/// A language model provider. Implementations keep the provider SDK out of Core.
/// </summary>
/// <remarks>
/// Provider failures are reported as <c>LlmProviderException</c>.
/// </remarks>
public interface ILlmClient
{
    /// <summary>
    /// Gets the provider name shown in the UI and logs, for example <c>OpenAI</c>.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Streams the model answer as text fragments in the order they are generated.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <param name="ct">Cancels the request; a new question cancels the previous answer.</param>
    /// <returns>Text fragments that form the full answer when concatenated.</returns>
    IAsyncEnumerable<string> StreamAsync(LlmRequest request, CancellationToken ct);

    /// <summary>
    /// Sends the request and deserializes the structured JSON answer.
    /// </summary>
    /// <typeparam name="T">The answer type; the JSON schema sent to the model is derived from it.</typeparam>
    /// <param name="request">The request to send.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <returns>The deserialized answer.</returns>
    Task<T> CompleteJsonAsync<T>(LlmRequest request, CancellationToken ct);
}
