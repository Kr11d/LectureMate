namespace LectureMate.Core.Llm;

/// <summary>
/// A progress notification for one answer, raised while the answer is generated.
/// </summary>
/// <param name="QaId">Identifier of the question-and-answer record the update belongs to.</param>
/// <param name="Kind">What happened.</param>
/// <param name="Text">Payload whose meaning depends on <paramref name="Kind"/>; see <see cref="AnswerUpdateKind"/>.</param>
public sealed record AnswerUpdate(Guid QaId, AnswerUpdateKind Kind, string Text);
