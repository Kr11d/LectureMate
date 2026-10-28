namespace LectureMate.Core.Domain;

/// <summary>
/// A block of captured audio in the single pipeline format described by <see cref="AudioFormats"/>
/// (16 kHz, mono, PCM 16-bit little-endian).
/// </summary>
/// <param name="Data">Raw PCM bytes.</param>
/// <param name="Timestamp">Position of the first sample, relative to the session start.</param>
public sealed record AudioChunk(ReadOnlyMemory<byte> Data, TimeSpan Timestamp);
