namespace LectureMate.Core.Domain;

/// <summary>
/// The single audio format used across the pipeline: 16 kHz, mono, PCM 16-bit,
/// and conversions between byte counts and durations in that format.
/// </summary>
public static class AudioFormats
{
    /// <summary>Samples per second.</summary>
    public const int SampleRate = 16_000;

    /// <summary>Number of channels.</summary>
    public const int Channels = 1;

    /// <summary>Bits per sample.</summary>
    public const int BitsPerSample = 16;

    /// <summary>Bytes in one sample frame (all channels).</summary>
    public const int BlockAlign = Channels * BitsPerSample / 8;

    /// <summary>Bytes in one second of audio.</summary>
    public const int BytesPerSecond = SampleRate * BlockAlign;

    /// <summary>
    /// Converts a byte count to the duration of the audio it holds.
    /// A trailing partial sample is ignored.
    /// </summary>
    /// <param name="bytes">Number of PCM bytes; must not be negative.</param>
    /// <returns>Duration of the whole samples contained in <paramref name="bytes"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bytes"/> is negative.</exception>
    public static TimeSpan BytesToDuration(int bytes)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(bytes);

        long samples = bytes / BlockAlign;
        return TimeSpan.FromTicks(samples * TimeSpan.TicksPerSecond / SampleRate);
    }

    /// <summary>
    /// Converts a duration to the number of PCM bytes needed to hold it,
    /// rounded down to a whole number of samples.
    /// </summary>
    /// <param name="duration">Duration; must not be negative.</param>
    /// <returns>A byte count that is a multiple of <see cref="BlockAlign"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="duration"/> is negative or too long to fit in <see cref="int"/> bytes.
    /// </exception>
    public static int DurationToBytes(TimeSpan duration)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(duration, TimeSpan.Zero);

        var samples = duration.Ticks / (TimeSpan.TicksPerSecond / SampleRate);
        var bytes = samples * BlockAlign;
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bytes, int.MaxValue, nameof(duration));
        return (int)bytes;
    }
}
