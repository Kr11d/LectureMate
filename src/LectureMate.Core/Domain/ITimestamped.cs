namespace LectureMate.Core.Domain;

/// <summary>
/// An item positioned on the session timeline. Times are measured from the start of the session.
/// </summary>
public interface ITimestamped
{
    /// <summary>Gets the moment the item starts, relative to the session start.</summary>
    TimeSpan Start { get; }

    /// <summary>Gets the moment the item ends, relative to the session start.</summary>
    TimeSpan End { get; }
}
