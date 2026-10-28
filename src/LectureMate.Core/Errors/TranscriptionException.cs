namespace LectureMate.Core.Errors;

/// <summary>
/// Thrown when the speech-to-text provider cannot be reached or returns an error.
/// </summary>
public sealed class TranscriptionException : LectureMateException
{
    /// <summary>Initializes a new instance with a message.</summary>
    /// <param name="message">Description of the error.</param>
    public TranscriptionException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance with a message and the exception that caused it.</summary>
    /// <param name="message">Description of the error.</param>
    /// <param name="innerException">The underlying exception.</param>
    public TranscriptionException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
