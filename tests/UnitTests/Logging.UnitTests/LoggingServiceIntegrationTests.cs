using Serilog.Events;
using Serilog.Sinks.TestCorrelator;

namespace Logging.UnitTests;

public class LoggingServiceIntegrationTests : BaseUnitTest
{
    public LoggingServiceIntegrationTests(ITestOutputHelper output) : base(output, LogEventLevel.Verbose) { }

    [Fact]
    public void ShouldLogDebugToUnitTestConsole()
    {
        using (TestCorrelator.CreateContext())
        {
            //_log.Verbose("This is a verbose string");
            _log.Debug("This is a debug string", 0);
            _log.Warning("This is a warning string", 0);
            _log.Information("This is an information string", 0);
            _log.Error("This is an error string", 0);
            _log.Fatal("This is a fatal string", 0);

            var logContext = TestCorrelator.GetLogEventsFromCurrentContext().ToList();
            logContext[0].Level.ShouldBe(LogEventLevel.Verbose);
            logContext[1].Level.ShouldBe(LogEventLevel.Debug);
            logContext[2].Level.ShouldBe(LogEventLevel.Warning);
            logContext[3].Level.ShouldBe(LogEventLevel.Information);
            logContext[4].Level.ShouldBe(LogEventLevel.Error);
            logContext[5].Level.ShouldBe(LogEventLevel.Fatal);
        }
    }

    [Fact]
    public void ShouldLogWithClassNameAndMethodName_WhenLoggingToUnitTestConsole()
    {
        using (TestCorrelator.CreateContext())
        {
            //_log.Verbose("This is a verbose string");
            // _log.Debug("This is a debug string", 0);
            // Log.Warning("This is a warning string");
            // _log.Information("This is an information string", 0);
            // Log.Error("This is an error string");
            // Log.Fatal("This is a fatal string");

            var position = new { Latitude = 25, Longitude = 134 };
            _log.Debug("[DEBUG LINE] - LogDebug message: {Position}", position);

            // var logContext = TestCorrelator.GetLogEventsFromCurrentContext().ToList();
            // logContext[0].Level.ShouldBe(LogEventLevel.Verbose);
            // logContext[1].Level.ShouldBe(LogEventLevel.Debug);
            // logContext[2].Level.ShouldBe(LogEventLevel.Warning);
            // logContext[3].Level.ShouldBe(LogEventLevel.Information);
            // logContext[4].Level.ShouldBe(LogEventLevel.Error);
            // logContext[5].Level.ShouldBe(LogEventLevel.Fatal);
        }
    }
}