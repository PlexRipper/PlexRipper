using Autofac;
using Reaparr.Environment;
using Serilog.Events;
using Serilog.Sinks.TestCorrelator;

namespace Reaparr.Logging.UnitTests;

public record TestLoginRequest
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required bool RememberMe { get; init; }
}

[NotInParallel]
public class LogExtensionsUnitTests : BaseUnitTest
{
    private ILogger CreateTestLogger(LogEventLevel logEventLevel) =>
        new TestLogConfig(Mock.Create<IPathProvider>()).GetLogger(logEventLevel).ForContext<LogExtensionsUnitTests>();

    [Test]
    public void ShouldLogTheSetLogLevel_WhenLogLevelSetIsVerbose()
    {
        // Arrange
        var log = CreateTestLogger(LogEventLevel.Verbose);

        // Act

        // Assert
        log.IsLogLevelEnabled(LogEventLevel.Verbose).ShouldBeTrue();
        log.IsLogLevelEnabled(LogEventLevel.Debug).ShouldBeTrue();
        log.IsLogLevelEnabled(LogEventLevel.Information).ShouldBeTrue();
        log.IsLogLevelEnabled(LogEventLevel.Warning).ShouldBeTrue();
        log.IsLogLevelEnabled(LogEventLevel.Error).ShouldBeTrue();
        log.IsLogLevelEnabled(LogEventLevel.Fatal).ShouldBeTrue();
    }

    [Test]
    public void ShouldNotLogTheSetLogLevel_WhenLogLevelIsAbove()
    {
        // Arrange
        var log = CreateTestLogger(LogEventLevel.Error);

        // Act

        // Assert
        log.IsLogLevelEnabled(LogEventLevel.Verbose).ShouldBeFalse();
        log.IsLogLevelEnabled(LogEventLevel.Debug).ShouldBeFalse();
        log.IsLogLevelEnabled(LogEventLevel.Information).ShouldBeFalse();
        log.IsLogLevelEnabled(LogEventLevel.Warning).ShouldBeFalse();
        log.IsLogLevelEnabled(LogEventLevel.Error).ShouldBeTrue();
        log.IsLogLevelEnabled(LogEventLevel.Fatal).ShouldBeTrue();
    }

    [Test]
    public void ShouldLogWithCorrectLogProperties_WhenEachLogTypeIsCalled()
    {
        // Arrange
        var position = new { Latitude = 25, Longitude = 134 };

        var log = CreateTestLogger(LogEventLevel.Verbose);

        using var context = TestCorrelator.CreateContext();

        // Act
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

        // Assert
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

    [Test]
    public void ShouldUsePluginMasking_WhenLoggingDebugApiCallInMaskedMode()
    {
        // Arrange
        var originalUnmaskedState = EnvironmentExtensions.IsUnmasked();
        EnvironmentExtensions.EnableUnmaskedLog(false);

        try
        {
            var pathProvider = Mock.Container.Resolve<IPathProvider>();
            LogFactory.CloseAndFlush();
            LogFactory.SetupLogging(new TestLogConfig(pathProvider), LogEventLevel.Debug);
            var log = LogFactory.Create<LogExtensionsUnitTests>();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = HttpMethods.Post;
            httpContext.Request.Scheme = "http";
            httpContext.Request.Host = new HostString("localhost", 5000);
            httpContext.Request.Path = "/api/Authentication/login";

            var request = new TestLoginRequest
            {
                Username = "ReaparrRocksDEV",
                Password = "TA%K3z3nr02AcI$0005N@88ps",
                RememberMe = false,
            };

            using var context = TestCorrelator.CreateContext();

            // Act
            log.DebugApiCall(httpContext, request);

            // Assert
            var logEvent = TestCorrelator.GetLogEventsFromContextId(context.Id).Single();
            var logMsg = logEvent.RenderMessage();

            logMsg.ShouldContain("Password");
            logMsg.ShouldContain("***MASKED***");
            logMsg.ShouldNotContain("ReaparrRocksDEV");
            logMsg.ShouldNotContain("TA%K3z3nr02AcI$0005N@88ps");
        }
        finally
        {
            LogFactory.CloseAndFlush();
            EnvironmentExtensions.EnableUnmaskedLog(originalUnmaskedState);
        }
    }

    [Test]
    public void ShouldLeavePasswordUnmasked_WhenLoggingDebugApiCallInUnmaskedMode()
    {
        // Arrange
        var originalUnmaskedState = EnvironmentExtensions.IsUnmasked();
        EnvironmentExtensions.EnableUnmaskedLog(true);

        try
        {
            var pathProvider = Mock.Container.Resolve<IPathProvider>();

            LogFactory.CloseAndFlush();
            LogFactory.SetupLogging(new TestLogConfig(pathProvider), LogEventLevel.Debug);

            var log = LogFactory.Create<LogExtensionsUnitTests>();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = HttpMethods.Post;
            httpContext.Request.Scheme = "http";
            httpContext.Request.Host = new HostString("localhost", 5000);
            httpContext.Request.Path = "/api/Authentication/login";

            var request = new TestLoginRequest
            {
                Username = "ReaparrRocksDEV",
                Password = "TA%K3z3nr02AcI$0005N@88ps",
                RememberMe = false,
            };

            using var context = TestCorrelator.CreateContext();

            // Act
            log.DebugApiCall(httpContext, request);

            // Assert
            var logEvent = TestCorrelator.GetLogEventsFromContextId(context.Id).Single();
            var logMsg = logEvent.RenderMessage();

            logMsg.ShouldContain("ReaparrRocksDEV");
            logMsg.ShouldContain("Password");
            logMsg.ShouldContain("TA%K3z3nr02AcI$0005N@88ps");
        }
        finally
        {
            LogFactory.CloseAndFlush();
            EnvironmentExtensions.EnableUnmaskedLog(originalUnmaskedState);
        }
    }
}
