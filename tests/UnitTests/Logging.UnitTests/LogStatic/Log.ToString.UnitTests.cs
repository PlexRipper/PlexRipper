namespace Reaparr.Logging.UnitTests;

public class LogToStringUnitTests : BaseUnitTest
{
    public LogToStringUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ShouldCorrectlyInterpolateStringResult_WhenToStringIsCalledOnLogEvent()
    {
        // Arrange

        var id = 1;
        var fileName = "test.txt";

        // Act
        var logEvent = Log.Here()
            .DebugMsg("Download worker with id: {Id} start for filename: {FileName}", id, fileName);

        // Assert
        logEvent.ShouldBe($"Download worker with id: {id} start for filename: \"{fileName}\"");
    }
}
