using Autofac.Extras.Moq;
using Logging;
using Logging.Interface;
using PlexRipper.BaseTests;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Logging.UnitTests;

public class LogToStringUnitTests : BaseUnitTest
{
    private readonly ILog<LogToStringUnitTests> _log;

    public LogToStringUnitTests(ITestOutputHelper output)
        : base(output)
    {
        _log = LogManager.CreateLogInstance<LogToStringUnitTests>(output);
    }

    [Fact]
    public void ShouldCorrectlyInterpolateStringResult_WhenToStringIsCalledOnLogEvent()
    {
        // Arrange

        var id = 1;
        var fileName = "test.txt";

        // Act
        var logEvent = _log.Here().Debug("Download worker with id: {Id} start for filename: {FileName}", id, fileName);
        var logString = logEvent.ToString();

        // Assert
        logString.ShouldBe($"Download worker with id: {id} start for filename: \"{fileName}\"");
    }
}
