namespace LectureMate.Core.Summary;

/// <summary>
/// A question from the lecture and its answer, as written in the summary.
/// </summary>
/// <param name="Question">The question text.</param>
/// <param name="Answer">The answer text.</param>
public sealed record QaPair(string Question, string Answer);
