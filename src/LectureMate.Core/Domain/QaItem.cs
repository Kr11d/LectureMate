namespace LectureMate.Core.Domain;

/// <summary>
/// A question together with the generated answer and the measured latencies.
/// </summary>
/// <param name="Id">Unique identifier.</param>
/// <param name="Question">The question that was answered.</param>
/// <param name="AnswerText">Answer text received so far, or the full answer.</param>
/// <param name="Status">Current status.</param>
/// <param name="Model">Name of the model that produced the answer.</param>
/// <param name="Latencies">Measured duration of each pipeline stage.</param>
public sealed record QaItem(Guid Id, QuestionCandidate Question, string AnswerText, QaStatus Status,
                            string Model, IReadOnlyDictionary<LatencyStage, TimeSpan> Latencies);
