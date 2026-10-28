namespace LectureMate.Core.Domain;

/// <summary>
/// State of a lecture session.
/// </summary>
public enum SessionState
{
    /// <summary>The session has not been started.</summary>
    Idle,

    /// <summary>Audio, transcriber and context are being prepared.</summary>
    Starting,

    /// <summary>Audio is captured and transcribed.</summary>
    Running,

    /// <summary>Capture is temporarily suspended.</summary>
    Paused,

    /// <summary>The session is shutting down.</summary>
    Stopping,

    /// <summary>The session has finished normally.</summary>
    Stopped,

    /// <summary>The session has stopped because of an unrecoverable error.</summary>
    Faulted,
}
