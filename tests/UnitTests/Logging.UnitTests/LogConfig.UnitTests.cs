using Reaparr.Environment;
using Serilog.Events;

namespace Reaparr.Logging.UnitTests;

[NotInParallel]
public class LogConfigUnitTests : BaseUnitTest<LogConfig>
{
    [Test]
    public void ShouldWritePlainTextToFile_WhenLoggerWritesToFileSink()
    {
        // Arrange
        var configDirectory = Path.Combine(
            Path.GetTempPath(),
            nameof(LogConfigUnitTests),
            nameof(ShouldWritePlainTextToFile_WhenLoggerWritesToFileSink)
        );
        var logsDirectory = Path.Combine(configDirectory, "Logs");

        if (Directory.Exists(configDirectory))
            Directory.Delete(configDirectory, true);

        Directory.CreateDirectory(logsDirectory);

        using var _ = WithEnvironmentVariablesAsync(
            new Dictionary<string, string?> { [EnvKeys.ReaparrConfigPath] = configDirectory }
        );

        // Act
        using (var logger = Sut.GetLogger(LogEventLevel.Information))
        {
            logger.ForContext<LogConfigUnitTests>().Information("File logging should stay plain text");
        }

        // Assert
        var logFilePath = Directory.GetFiles(logsDirectory, "log*.txt").SingleOrDefault();
        logFilePath.ShouldNotBeNull();

        var logContents = File.ReadAllText(logFilePath);
        logContents.ShouldContain("File logging should stay plain text");
        logContents.ShouldNotContain("\u001b[");
    }
}
