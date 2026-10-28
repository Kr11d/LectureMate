namespace LectureMate.Core.Llm;

/// <summary>
/// How an answer was requested. Controls whether the model may decline with <c>NO_QUESTION</c>.
/// </summary>
public enum AnswerMode
{
    /// <summary>
    /// Requested by the question detector. The model may reply <c>NO_QUESTION</c>,
    /// and then the answer is discarded.
    /// </summary>
    Auto,

    /// <summary>
    /// Requested by the user with a hotkey or a button. The marker is not used,
    /// so the model always answers.
    /// </summary>
    Manual,
}
