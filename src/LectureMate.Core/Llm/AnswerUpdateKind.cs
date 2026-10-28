namespace LectureMate.Core.Llm;

/// <summary>
/// The kind of progress reported for an answer. Also defines what <see cref="AnswerUpdate.Text"/> holds.
/// </summary>
public enum AnswerUpdateKind
{
    /// <summary>
    /// The request was sent. Text is empty.
    /// </summary>
    Started,

    /// <summary>
    /// A new fragment arrived. Text holds only the fragment, to be appended to what is shown.
    /// </summary>
    Token,

    /// <summary>
    /// The answer is complete. Text holds the full answer.
    /// </summary>
    Completed,

    /// <summary>
    /// The model replied <c>NO_QUESTION</c>, so the answer is not shown. Text is empty.
    /// </summary>
    Discarded,

    /// <summary>
    /// The request failed or timed out. Text holds a technical description for the log;
    /// the UI shows its own localized message.
    /// </summary>
    Failed,
}
