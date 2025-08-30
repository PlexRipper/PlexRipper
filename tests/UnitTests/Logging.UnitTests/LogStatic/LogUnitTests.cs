using Reaparr.BaseTests;
using Reaparr.Logging.Interface;
using Serilog.Events;

namespace Reaparr.Logging.UnitTests;

public class LogUnitTests : BaseUnitTest<LogUnitTests>
{
    private readonly ILog<LogUnitTests> _log;
    private readonly ILog _logEmpty;

    public LogUnitTests(ITestOutputHelper output)
        : base(output)
    {
        _log = new TestLogConfig(output).CreateLogInstance<LogUnitTests>();
        _logEmpty = new TestLogConfig(output).CreateLogInstance();
    }

    [Fact]
    public void ShouldLogTheSetLogLevel_WhenLogLevelSetIsVerbose()
    {
        // Arrange

        // Act
        var logLevelSet = Log.IsLogLevelEnabled(LogEventLevel.Verbose);

        // Assert

        logLevelSet.ShouldBeTrue();
    }

    [Fact]
    public void ShouldLogWithCorrectLogLevel_WhenEachLogTypeIsCalled()
    {
        var position = new { Latitude = 25, Longitude = 134 };

        var verboseLogEvent = _log.Verbose(
            "This is a verbose string with a json object: {Position}, a number {Count}, a bool: {Boolean}",
            position,
            9999,
            true
        );
        var debugLogEvent = _log.Debug(
            "This is a debug string with a json object: {Position}, a number {Count}, a bool: {Boolean}",
            position,
            9999,
            true
        );
        var warningLogEvent = _log.Warning(
            "This is a warning string with a json object: {Position}, a number {Count}, a bool: {Boolean}",
            position,
            9999,
            true
        );
        var informationLogEvent = _log.Information(
            "This is an information string with a json object: {Position}, a number {Count}, a bool: {Boolean}",
            position,
            9999,
            true
        );
        var errorLogEvent = _log.Error(
            "This is an error string with a json object: {Position}, a number {Count}, a bool: {Boolean}",
            position,
            9999,
            true
        );
        var fatalLogEvent = _log.Fatal(
            "This is a fatal string with a json object: {Position}, a number {Count}, a bool: {Boolean}",
            position,
            9999,
            true
        );

        verboseLogEvent.LogLevel.ShouldBe(LogEventLevel.Verbose);
        debugLogEvent.LogLevel.ShouldBe(LogEventLevel.Debug);
        warningLogEvent.LogLevel.ShouldBe(LogEventLevel.Warning);
        informationLogEvent.LogLevel.ShouldBe(LogEventLevel.Information);
        errorLogEvent.LogLevel.ShouldBe(LogEventLevel.Error);
        fatalLogEvent.LogLevel.ShouldBe(LogEventLevel.Fatal);
    }

    [Fact]
    public void ShouldLogWithClassNameAndMethodName_WhenLoggingToUnitTestConsole()
    {
        var position = new { Latitude = 25, Longitude = 134 };

        var verboseLogEvent = _log.Verbose(
            "This is a verbose string with a json object: {Position}, a number {Count}, a bool: {Boolean}",
            position,
            9999,
            true
        );
        var debugLogEvent = _log.Debug(
            "This is a debug string with a json object: {Position}, a number {Count}, a bool: {Boolean}",
            position,
            9999,
            true
        );
        var warningLogEvent = _log.Warning(
            "This is a warning string with a json object: {Position}, a number {Count}, a bool: {Boolean}",
            position,
            9999,
            true
        );
        var informationLogEvent = _log.Information(
            "This is an information string with a json object: {Position}, a number {Count}, a bool: {Boolean}",
            position,
            9999,
            true
        );
        var errorLogEvent = _log.Error(
            "This is an error string with a json object: {Position}, a number {Count}, a bool: {Boolean}",
            position,
            9999,
            true
        );
        var fatalLogEvent = _log.Fatal(
            "This is a fatal string with a json object: {Position}, a number {Count}, a bool: {Boolean}",
            position,
            9999,
            true
        );

        verboseLogEvent.LogLevel.ShouldBe(LogEventLevel.Verbose);
        debugLogEvent.LogLevel.ShouldBe(LogEventLevel.Debug);
        warningLogEvent.LogLevel.ShouldBe(LogEventLevel.Warning);
        informationLogEvent.LogLevel.ShouldBe(LogEventLevel.Information);
        errorLogEvent.LogLevel.ShouldBe(LogEventLevel.Error);
        fatalLogEvent.LogLevel.ShouldBe(LogEventLevel.Fatal);

        verboseLogEvent.ClassName.ShouldBe(nameof(LogUnitTests));
        debugLogEvent.ClassName.ShouldBe(nameof(LogUnitTests));
        warningLogEvent.ClassName.ShouldBe(nameof(LogUnitTests));
        informationLogEvent.ClassName.ShouldBe(nameof(LogUnitTests));
        errorLogEvent.ClassName.ShouldBe(nameof(LogUnitTests));
        fatalLogEvent.ClassName.ShouldBe(nameof(LogUnitTests));

        verboseLogEvent.MethodName.ShouldBe(nameof(ShouldLogWithClassNameAndMethodName_WhenLoggingToUnitTestConsole));
        debugLogEvent.MethodName.ShouldBe(nameof(ShouldLogWithClassNameAndMethodName_WhenLoggingToUnitTestConsole));
        warningLogEvent.MethodName.ShouldBe(nameof(ShouldLogWithClassNameAndMethodName_WhenLoggingToUnitTestConsole));
        informationLogEvent.MethodName.ShouldBe(
            nameof(ShouldLogWithClassNameAndMethodName_WhenLoggingToUnitTestConsole)
        );
        errorLogEvent.MethodName.ShouldBe(nameof(ShouldLogWithClassNameAndMethodName_WhenLoggingToUnitTestConsole));
        fatalLogEvent.MethodName.ShouldBe(nameof(ShouldLogWithClassNameAndMethodName_WhenLoggingToUnitTestConsole));
    }

    [Fact]
    public void ShouldLogWithClassNameAndMethodName_WhenLoggingWithHereToUnitTestConsole()
    {
        var position = new { Latitude = 25, Longitude = 134 };

        var verboseLogEvent = _logEmpty
            .Here()
            .Verbose(
                "This is a verbose string with a json object: {Position}, a number {Count}, a bool: {Boolean}",
                position,
                9999,
                true
            );
        var debugLogEvent = _logEmpty
            .Here()
            .Debug(
                "This is a debug string with a json object: {Position}, a number {Count}, a bool: {Boolean}",
                position,
                9999,
                true
            );
        var warningLogEvent = _logEmpty
            .Here()
            .Warning(
                "This is a warning string with a json object: {Position}, a number {Count}, a bool: {Boolean}",
                position,
                9999,
                true
            );
        var informationLogEvent = _logEmpty
            .Here()
            .Information(
                "This is an information string with a json object: {Position}, a number {Count}, a bool: {Boolean}",
                position,
                9999,
                true
            );
        var errorLogEvent = _logEmpty
            .Here()
            .Error(
                "This is an error string with a json object: {Position}, a number {Count}, a bool: {Boolean}",
                position,
                9999,
                true
            );
        var fatalLogEvent = _logEmpty
            .Here()
            .Fatal(
                "This is a fatal string with a json object: {Position}, a number {Count}, a bool: {Boolean}",
                position,
                9999,
                true
            );

        verboseLogEvent.LogLevel.ShouldBe(LogEventLevel.Verbose);
        debugLogEvent.LogLevel.ShouldBe(LogEventLevel.Debug);
        warningLogEvent.LogLevel.ShouldBe(LogEventLevel.Warning);
        informationLogEvent.LogLevel.ShouldBe(LogEventLevel.Information);
        errorLogEvent.LogLevel.ShouldBe(LogEventLevel.Error);
        fatalLogEvent.LogLevel.ShouldBe(LogEventLevel.Fatal);

        verboseLogEvent.ClassName.ShouldBe(nameof(LogUnitTests));
        debugLogEvent.ClassName.ShouldBe(nameof(LogUnitTests));
        warningLogEvent.ClassName.ShouldBe(nameof(LogUnitTests));
        informationLogEvent.ClassName.ShouldBe(nameof(LogUnitTests));
        errorLogEvent.ClassName.ShouldBe(nameof(LogUnitTests));
        fatalLogEvent.ClassName.ShouldBe(nameof(LogUnitTests));

        verboseLogEvent.MethodName.ShouldBe(
            nameof(ShouldLogWithClassNameAndMethodName_WhenLoggingWithHereToUnitTestConsole)
        );
        debugLogEvent.MethodName.ShouldBe(
            nameof(ShouldLogWithClassNameAndMethodName_WhenLoggingWithHereToUnitTestConsole)
        );
        warningLogEvent.MethodName.ShouldBe(
            nameof(ShouldLogWithClassNameAndMethodName_WhenLoggingWithHereToUnitTestConsole)
        );
        informationLogEvent.MethodName.ShouldBe(
            nameof(ShouldLogWithClassNameAndMethodName_WhenLoggingWithHereToUnitTestConsole)
        );
        errorLogEvent.MethodName.ShouldBe(
            nameof(ShouldLogWithClassNameAndMethodName_WhenLoggingWithHereToUnitTestConsole)
        );
        fatalLogEvent.MethodName.ShouldBe(
            nameof(ShouldLogWithClassNameAndMethodName_WhenLoggingWithHereToUnitTestConsole)
        );
    }
}
