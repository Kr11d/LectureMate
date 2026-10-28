using LectureMate.Core.Errors;

namespace LectureMate.Core.Tests.Errors;

public class LlmProviderExceptionTests
{
    [Theory]
    [InlineData(400, null, false)]
    [InlineData(401, null, false)]
    [InlineData(403, null, false)]
    [InlineData(429, null, true)]
    [InlineData(500, null, true)]
    [InlineData(503, null, true)]
    [InlineData(null, true, true)]
    [InlineData(null, false, false)]
    public void IsRetryable_DependsOnStatusCodeOrConstructorArgument(int? statusCode, bool? isRetryable, bool expected)
    {
        var exception = statusCode is { } code
            ? new LlmProviderException("error", code)
            : new LlmProviderException("timeout", isRetryable!.Value);

        Assert.Equal(statusCode, exception.StatusCode);
        Assert.Equal(expected, exception.IsRetryable);
    }

    [Fact]
    public void Constructor_KeepsMessageAndInnerException()
    {
        var inner = new TimeoutException();

        var exception = new LlmProviderException("timeout", isRetryable: true, inner);

        Assert.Equal("timeout", exception.Message);
        Assert.Same(inner, exception.InnerException);
    }

    [Fact]
    public void IsLectureMateException()
    {
        Assert.IsAssignableFrom<LectureMateException>(new LlmProviderException("error", 500));
    }
}
