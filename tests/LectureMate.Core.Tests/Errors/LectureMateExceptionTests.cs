using LectureMate.Core.Errors;

namespace LectureMate.Core.Tests.Errors;

public class LectureMateExceptionTests
{
    private static readonly InvalidOperationException _inner = new();

    public static TheoryData<LectureMateException> Descendants =>
    [
        new AudioCaptureException("message", _inner),
        new TranscriptionException("message", _inner),
        new ContextLoadException("message", _inner),
        new LlmProviderException("message", 500, _inner),
    ];

    [Theory]
    [MemberData(nameof(Descendants))]
    public void Descendant_KeepsMessageAndInnerException(LectureMateException exception)
    {
        Assert.Equal("message", exception.Message);
        Assert.Same(_inner, exception.InnerException);
    }
}
