namespace LectureMate.Core.Errors;

/// <summary>
/// Thrown when a language model provider returns an error or does not respond in time.
/// </summary>
public sealed class LlmProviderException : LectureMateException
{
    /// <summary>
    /// Initializes a new instance for an HTTP error response.
    /// <see cref="IsRetryable"/> is derived from <paramref name="statusCode"/>:
    /// 429 and 5xx are retryable, everything else (400, 401, 403, …) is not.
    /// </summary>
    /// <param name="message">Description of the error.</param>
    /// <param name="statusCode">HTTP status code returned by the provider.</param>
    /// <param name="innerException">The underlying exception.</param>
    public LlmProviderException(string message, int statusCode, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        IsRetryable = IsRetryableStatusCode(statusCode);
    }

    /// <summary>
    /// Initializes a new instance for an error without an HTTP status code,
    /// for example a timeout or a dropped connection.
    /// </summary>
    /// <param name="message">Description of the error.</param>
    /// <param name="isRetryable">Whether repeating the request may succeed.</param>
    /// <param name="innerException">The underlying exception.</param>
    public LlmProviderException(string message, bool isRetryable, Exception? innerException = null)
        : base(message, innerException)
    {
        IsRetryable = isRetryable;
    }

    /// <summary>Gets the HTTP status code, or <see langword="null"/> if the error has none.</summary>
    public int? StatusCode { get; }

    /// <summary>Gets a value indicating whether repeating the request may succeed.</summary>
    public bool IsRetryable { get; }

    private static bool IsRetryableStatusCode(int statusCode) =>
        statusCode == 429 || statusCode is >= 500 and <= 599;
}
