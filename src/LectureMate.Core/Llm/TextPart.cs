namespace LectureMate.Core.Llm;

/// <summary>
/// A text fragment of a model request.
/// </summary>
/// <param name="Text">The text sent to the model as is.</param>
public sealed record TextPart(string Text) : MessagePart;
