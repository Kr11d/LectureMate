namespace LectureMate.Core.Domain;

/// <summary>
/// Lifecycle status of a <see cref="QaItem"/>.
/// </summary>
public enum QaStatus
{
    /// <summary>The answer is being streamed.</summary>
    Answering,

    /// <summary>The answer is complete.</summary>
    Answered,

    /// <summary>The model decided the fragment is not a question for students.</summary>
    Discarded,

    /// <summary>The answer could not be produced because of an error.</summary>
    Failed,
}
