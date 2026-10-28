namespace LectureMate.Core.Errors;

/// <summary>
/// Base class for all errors raised by LectureMate. Catch this type to handle any application error.
/// </summary>
public abstract class LectureMateException : Exception
{
    /// <summary>Initializes a new instance with a message.</summary>
    /// <param name="message">Description of the error.</param>
    protected LectureMateException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance with a message and the exception that caused it.</summary>
    /// <param name="message">Description of the error.</param>
    /// <param name="innerException">The underlying exception.</param>
    protected LectureMateException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
