namespace LectureMate.Core.Domain;

/// <summary>
/// A fragment of the transcript that is treated as a lecturer's question.
/// </summary>
/// <param name="Text">Text of the question.</param>
/// <param name="Start">Start of the question, relative to the session start.</param>
/// <param name="End">End of the question, relative to the session start.</param>
/// <param name="Score">Detector score; higher means more likely a question.</param>
/// <param name="Source">How the candidate was produced.</param>
public sealed record QuestionCandidate(string Text, TimeSpan Start, TimeSpan End,
                                       double Score, QuestionSource Source) : ITimestamped;
