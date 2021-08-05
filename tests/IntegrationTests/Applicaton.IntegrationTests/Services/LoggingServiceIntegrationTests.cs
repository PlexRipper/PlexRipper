using System.Linq;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using Serilog.Events;
using Serilog.Sinks.TestCorrelator;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace PlexRipper.Application.IntegrationTests.Services
{
    public class LoggingServiceIntegrationTests
    {
        public LoggingServiceIntegrationTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.SetupLogging(output);
        }

        [Fact]
        public void ShouldLogDebugToUnitTestConsole()
        {
            using (TestCorrelator.CreateContext())
            {
                Log.Verbose("This is a verbose string");
                Log.Debug("This is a debug string");
                Log.Warning("This is a warning string");
                Log.Information("This is an information string");
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
}