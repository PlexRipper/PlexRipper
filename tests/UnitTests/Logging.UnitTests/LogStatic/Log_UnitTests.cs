using Serilog.Events;

namespace Logging.UnitTests;

public class LogUnitTests : BaseUnitTest
{
    public LogUnitTests(ITestOutputHelper output) : base(output, LogEventLevel.Verbose) { }

    [Fact]
    public void ShouldLogTheSetLogLevel_WhenLogLevelSetIsVerbose()
    {
        // Arrange

        // Act
        var logLevelSet = _log.IsLogLevelEnabled(LogEventLevel.Verbose);
        var logStaticLevelSet = LogStatic.LogStatic.IsLogLevelEnabled(LogEventLevel.Verbose);

        // Assert

        logLevelSet.ShouldBeTrue();
        logStaticLevelSet.ShouldBeTrue();
    }

    [Fact]
    public void ShouldLogWithCorrectLogLevel_WhenEachLogTypeIsCalled()
    {
        var position = new { Latitude = 25, Longitude = 134 };

        var verboseLogEvent = _log.Verbose("This is a verbose string with a json object: {Position}, a number {Count}", position, 9999);
        var debugLogEvent = _log.Debug("This is a debug string with a json object: {Position}, a number {Count}", position, 9999);
        var warningLogEvent = _log.Warning("This is a warning string with a json object: {Position}, a number {Count}", position, 9999);
        var informationLogEvent = _log.Information("This is an information string with a json object: {Position}, a number {Count}", position, 9999);
        var errorLogEvent = _log.Error("This is an error string with a json object: {Position}, a number {Count}", position, 9999);
        var fatalLogEvent = _log.Fatal("This is a fatal string with a json object: {Position}, a number {Count}", position, 9999);

        verboseLogEvent.Level.ShouldBe(LogEventLevel.Verbose);
        debugLogEvent.Level.ShouldBe(LogEventLevel.Debug);
        warningLogEvent.Level.ShouldBe(LogEventLevel.Warning);
        informationLogEvent.Level.ShouldBe(LogEventLevel.Information);
        errorLogEvent.Level.ShouldBe(LogEventLevel.Error);
        fatalLogEvent.Level.ShouldBe(LogEventLevel.Fatal);
    }

    [Fact]
    public void ShouldLogWithClassNameAndMethodName_WhenLoggingToUnitTestConsole()
    {
        var position = new { Latitude = 25, Longitude = 134 };

        var verboseLogEvent = _log.Verbose("This is a verbose string with a json object: {Position}, a number {Count}", position, 9999);
        var debugLogEvent = _log.Debug("This is a debug string with a json object: {Position}, a number {Count}", position, 9999);
        var warningLogEvent = _log.Warning("This is a warning string with a json object: {Position}, a number {Count}", position, 9999);
        var informationLogEvent = _log.Information("This is an information string with a json object: {Position}, a number {Count}", position, 9999);
        var errorLogEvent = _log.Error("This is an error string with a json object: {Position}, a number {Count}", position, 9999);
        var fatalLogEvent = _log.Fatal("This is a fatal string with a json object: {Position}, a number {Count}", position, 9999);

        verboseLogEvent.GetClassName().ShouldBe(nameof(LogUnitTests));
        debugLogEvent.GetClassName().ShouldBe(nameof(LogUnitTests));
        warningLogEvent.GetClassName().ShouldBe(nameof(LogUnitTests));
        informationLogEvent.GetClassName().ShouldBe(nameof(LogUnitTests));
        errorLogEvent.GetClassName().ShouldBe(nameof(LogUnitTests));
        fatalLogEvent.GetClassName().ShouldBe(nameof(LogUnitTests));

        verboseLogEvent.GetMethodName().ShouldBe(nameof(ShouldLogWithClassNameAndMethodName_WhenLoggingToUnitTestConsole));
        debugLogEvent.GetMethodName().ShouldBe(nameof(ShouldLogWithClassNameAndMethodName_WhenLoggingToUnitTestConsole));
        warningLogEvent.GetMethodName().ShouldBe(nameof(ShouldLogWithClassNameAndMethodName_WhenLoggingToUnitTestConsole));
        informationLogEvent.GetMethodName().ShouldBe(nameof(ShouldLogWithClassNameAndMethodName_WhenLoggingToUnitTestConsole));
        errorLogEvent.GetMethodName().ShouldBe(nameof(ShouldLogWithClassNameAndMethodName_WhenLoggingToUnitTestConsole));
        fatalLogEvent.GetMethodName().ShouldBe(nameof(ShouldLogWithClassNameAndMethodName_WhenLoggingToUnitTestConsole));
    }
}