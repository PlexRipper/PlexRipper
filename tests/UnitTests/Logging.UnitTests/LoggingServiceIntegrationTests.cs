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
            Log.Warning("This is a warning string");
            _log.Information("This is an information string", 0);
            Log.Error("This is an error string");
            Log.Fatal("This is a fatal string");

            var logContext = TestCorrelator.GetLogEventsFromCurrentContext().ToList();
            logContext[0].Level.ShouldBe(LogEventLevel.Verbose);
            logContext[1].Level.ShouldBe(LogEventLevel.Debug);
            logContext[2].Level.ShouldBe(LogEventLevel.Warning);
            logContext[3].Level.ShouldBe(LogEventLevel.Information);
            logContext[4].Level.ShouldBe(LogEventLevel.Error);
            logContext[5].Level.ShouldBe(LogEventLevel.Fatal);
        }
    }
}