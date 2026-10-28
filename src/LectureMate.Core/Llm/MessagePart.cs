namespace LectureMate.Core.Llm;

/// <summary>
/// A piece of content sent to a language model: text or an image.
/// </summary>
/// <remarks>
/// Provider clients translate each part into the provider's own content block,
/// so Core never depends on a provider SDK.
/// </remarks>
public abstract record MessagePart;
