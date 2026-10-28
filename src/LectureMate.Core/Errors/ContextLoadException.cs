namespace LectureMate.Core.Errors;

/// <summary>
/// Thrown when a course material cannot be loaded, for example an unreadable or encrypted PDF.
/// </summary>
public sealed class ContextLoadException : LectureMateException
{
    /// <summary>Initializes a new instance with a message.</summary>
    /// <param name="message">Description of the error.</param>
    public ContextLoadException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance with a message and the exception that caused it.</summary>
    /// <param name="message">Description of the error.</param>
    /// <param name="innerException">The underlying exception.</param>
    public ContextLoadException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
