using Logging.Interface;
using Serilog.Events;

namespace Logging.UnitTests;

public class Log_UnitTests : BaseUnitTest<Log_UnitTests>
{
    private readonly ILog<Log_UnitTests> _log;
    private readonly ILog _logEmpty;

    public Log_UnitTests(ITestOutputHelper output)
        : base(output, LogEventLevel.Verbose)
    {
        _log = LogManager.CreateLogInstance<Log_UnitTests>(output, LogEventLevel.Verbose);
        _logEmpty = LogManager.CreateLogInstance(output, LogEventLevel.Verbose);
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

        verboseLogEvent.ClassName.ShouldBe(nameof(Log_UnitTests));
        debugLogEvent.ClassName.ShouldBe(nameof(Log_UnitTests));
        warningLogEvent.ClassName.ShouldBe(nameof(Log_UnitTests));
        informationLogEvent.ClassName.ShouldBe(nameof(Log_UnitTests));
        errorLogEvent.ClassName.ShouldBe(nameof(Log_UnitTests));
        fatalLogEvent.ClassName.ShouldBe(nameof(Log_UnitTests));

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

        verboseLogEvent.ClassName.ShouldBe(nameof(Log_UnitTests));
        debugLogEvent.ClassName.ShouldBe(nameof(Log_UnitTests));
        warningLogEvent.ClassName.ShouldBe(nameof(Log_UnitTests));
        informationLogEvent.ClassName.ShouldBe(nameof(Log_UnitTests));
        errorLogEvent.ClassName.ShouldBe(nameof(Log_UnitTests));
        fatalLogEvent.ClassName.ShouldBe(nameof(Log_UnitTests));

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
