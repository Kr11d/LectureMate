using LectureMate.Core.Domain;

namespace LectureMate.Core.Tests.Domain;

public class AudioFormatsTests
{
    [Fact]
    public void Constants_DescribePipelineFormat()
    {
        Assert.Equal(16_000, AudioFormats.SampleRate);
        Assert.Equal(1, AudioFormats.Channels);
        Assert.Equal(16, AudioFormats.BitsPerSample);
        Assert.Equal(2, AudioFormats.BlockAlign);
        Assert.Equal(32_000, AudioFormats.BytesPerSecond);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(2, 625)]                // one sample = 62.5 µs
    [InlineData(32_000, 10_000_000)]    // one second
    [InlineData(320_000, 100_000_000)]  // ten seconds
    public void BytesToDuration_WholeSamples_ReturnsExactDuration(int bytes, long expectedTicks)
    {
        Assert.Equal(TimeSpan.FromTicks(expectedTicks), AudioFormats.BytesToDuration(bytes));
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(3, 2)]
    [InlineData(32_001, 32_000)]
    public void BytesToDuration_OddBytes_IgnoresPartialSample(int bytes, int alignedBytes)
    {
        Assert.Equal(AudioFormats.BytesToDuration(alignedBytes), AudioFormats.BytesToDuration(bytes));
    }

    [Fact]
    public void BytesToDuration_Negative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AudioFormats.BytesToDuration(-1));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(625, 2)]
    [InlineData(10_000_000, 32_000)]
    [InlineData(1_000_000, 3_200)]      // 100 ms
    public void DurationToBytes_WholeSamples_ReturnsExactBytes(long ticks, int expectedBytes)
    {
        Assert.Equal(expectedBytes, AudioFormats.DurationToBytes(TimeSpan.FromTicks(ticks)));
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(624, 0)]
    [InlineData(626, 2)]
    [InlineData(1_249, 2)]
    public void DurationToBytes_PartialSample_RoundsDownToWholeSample(long ticks, int expectedBytes)
    {
        var bytes = AudioFormats.DurationToBytes(TimeSpan.FromTicks(ticks));

        Assert.Equal(expectedBytes, bytes);
        Assert.Equal(0, bytes % AudioFormats.BlockAlign);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(3_200)]
    [InlineData(32_000)]
    public void DurationToBytes_RoundTripsBytesToDuration(int bytes)
    {
        Assert.Equal(bytes, AudioFormats.DurationToBytes(AudioFormats.BytesToDuration(bytes)));
    }

    [Fact]
    public void DurationToBytes_Negative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AudioFormats.DurationToBytes(TimeSpan.FromTicks(-1)));
    }

    [Fact]
    public void DurationToBytes_TooLongForInt_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AudioFormats.DurationToBytes(TimeSpan.FromDays(1)));
    }
}
