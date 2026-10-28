namespace LectureMate.Core.Domain;

/// <summary>
/// A piece of recognized speech returned by a transcriber.
/// </summary>
/// <param name="Text">Recognized text.</param>
/// <param name="Start">Start of the segment, relative to the session start.</param>
/// <param name="End">End of the segment, relative to the session start.</param>
/// <param name="IsFinal"><see langword="true"/> for a final result; <see langword="false"/> for a partial one that may still change.</param>
/// <param name="Language">Language code of the segment, for example <c>uk</c> or <c>en</c>.</param>
public sealed record TranscriptSegment(string Text, TimeSpan Start, TimeSpan End,
                                       bool IsFinal, string Language) : ITimestamped;
