namespace LectureMate.Core.Errors;

/// <summary>
/// Thrown when audio capture fails, for example when no device is available or the device disappears.
/// </summary>
public sealed class AudioCaptureException : LectureMateException
{
    /// <summary>Initializes a new instance with a message.</summary>
    /// <param name="message">Description of the error.</param>
    public AudioCaptureException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance with a message and the exception that caused it.</summary>
    /// <param name="message">Description of the error.</param>
    /// <param name="innerException">The underlying exception.</param>
    public AudioCaptureException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
