using Serilog.Events;
using Serilog.Sinks.TestCorrelator;

namespace Reaparr.Logging.UnitTests;

public class LogExtensionsUnitTests
{
    private readonly ITestOutputHelper _output;

    public LogExtensionsUnitTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ShouldLogTheSetLogLevel_WhenLogLevelSetIsVerbose()
    {
        // Arrange
        var log = new TestLogConfig(_output).GetLogger(LogEventLevel.Verbose).ForContext<LogExtensionsUnitTests>();

        // Assert
        log.IsLogLevelEnabled(LogEventLevel.Verbose).ShouldBeTrue();
        log.IsLogLevelEnabled(LogEventLevel.Debug).ShouldBeTrue();
        log.IsLogLevelEnabled(LogEventLevel.Information).ShouldBeTrue();
        log.IsLogLevelEnabled(LogEventLevel.Warning).ShouldBeTrue();
        log.IsLogLevelEnabled(LogEventLevel.Error).ShouldBeTrue();
        log.IsLogLevelEnabled(LogEventLevel.Fatal).ShouldBeTrue();
    }

    [Fact]
    public void ShouldNotLogTheSetLogLevel_WhenLogLevelIsAbove()
    {
        // Arrange
        var log = new TestLogConfig(_output).GetLogger(LogEventLevel.Error).ForContext<LogExtensionsUnitTests>();

        // Assert
        log.IsLogLevelEnabled(LogEventLevel.Verbose).ShouldBeFalse();
        log.IsLogLevelEnabled(LogEventLevel.Debug).ShouldBeFalse();
        log.IsLogLevelEnabled(LogEventLevel.Information).ShouldBeFalse();
        log.IsLogLevelEnabled(LogEventLevel.Warning).ShouldBeFalse();
        log.IsLogLevelEnabled(LogEventLevel.Error).ShouldBeTrue();
        log.IsLogLevelEnabled(LogEventLevel.Fatal).ShouldBeTrue();
    }

    [Fact]
    public void ShouldLogWithCorrectLogProperties_WhenEachLogTypeIsCalled()
    {
        var position = new { Latitude = 25, Longitude = 134 };

        var log = new TestLogConfig(_output).GetLogger(LogEventLevel.Verbose).ForContext<LogExtensionsUnitTests>();

        using var context = TestCorrelator.CreateContext();

        log.Here()
            .VerboseMsg(
                "This is a verbose string with a json object: {Position}, a number {Count}, a bool: {Boolean}",
                position,
                9999,
                true
            );
        log.Here()
            .DebugMsg(
                "This is a debug string with a json object: {Position}, a number {Count}, a bool: {Boolean}",
                position,
                9999,
                true
            );
        log.Here()
            .WarningMsg(
                "This is a warning string with a json object: {Position}, a number {Count}, a bool: {Boolean}",
                position,
                9999,
                true
            );
        log.Here()
            .InformationMsg(
                "This is a information string with a json object: {Position}, a number {Count}, a bool: {Boolean}",
                position,
                9999,
                true
            );
        log.Here()
            .ErrorMsg(
                "This is a error string with a json object: {Position}, a number {Count}, a bool: {Boolean}",
                position,
                9999,
                true
            );
        log.Here()
            .FatalMsg(
                "This is a fatal string with a json object: {Position}, a number {Count}, a bool: {Boolean}",
                position,
                9999,
                true
            );

        var logEvents = TestCorrelator.GetLogEventsFromContextId(context.Id).ToList();
        logEvents.ShouldNotBeEmpty();

        foreach (var logEvent in logEvents)
        {
            var sourceContext = logEvent.GetStringProperty(LogConfig.SourceContext);
            sourceContext.ShouldNotBeNull();
            sourceContext.ShouldContain(nameof(LogExtensionsUnitTests));

            var fileName = logEvent.GetStringProperty(LogConfig.FileName);
            fileName.ShouldNotBeNull();
            fileName.ShouldContain("LogExtensions.UnitTests");

            var methodName = logEvent.GetStringProperty(LogConfig.MethodName);
            methodName.ShouldNotBeNull();
            methodName.ShouldBe(nameof(ShouldLogWithCorrectLogProperties_WhenEachLogTypeIsCalled));

            var lineNumber = logEvent.GetIntProperty(LogConfig.LineNumber);
            lineNumber.ShouldNotBeNull();
            lineNumber.ShouldNotBe(0);

            var logMsg = logEvent.RenderMessage();
            logMsg.ShouldContain(
                $"This is a {logEvent.Level} string with a json object: \"{{ Latitude = 25, Longitude = 134 }}\", a number 9999, a bool: true"
            );
        }
    }
}
