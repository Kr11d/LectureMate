namespace LectureMate.Core.Llm;

/// <summary>
/// An image fragment of a model request, such as a slide or a screenshot.
/// </summary>
/// <param name="Bytes">Encoded image (for example PNG or JPEG), already scaled to the provider limit.</param>
/// <param name="MediaType">MIME type of <paramref name="Bytes"/>, for example <c>image/png</c>.</param>
/// <remarks>
/// Record equality compares <paramref name="Bytes"/> as a reference to the same buffer,
/// not by content: two parts built from different arrays with equal bytes are not equal.
/// </remarks>
public sealed record ImagePart(ReadOnlyMemory<byte> Bytes, string MediaType) : MessagePart;
