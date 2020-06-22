using PlexRipper.BaseTests;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace PlexRipper.Application.IntegrationTests.Services
{
    public class LoggingServiceIntegrationTests
    {
        private BaseContainer Container { get; }

        public LoggingServiceIntegrationTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.Setup(output);
            Container = new BaseContainer();
        }

        [Fact]
        public void ShouldLogDebugToUnitTestConsole()
        {
            var logger = BaseDependanciesTest.GetLogger<object>();
            logger.Verbose("This is a verbose string");
            logger.Debug("This is a debug string");
            logger.Warning("This is a warning string");
            logger.Information("This is an information string");
            logger.Error("This is an error string");
            logger.Fatal("This is a fatal string");

            logger = Log.Logger;
            logger.Verbose("Log.Logger => This is a verbose string");
            logger.Debug("Log.Logger => This is a debug string");
            logger.Warning("Log.Logger => This is a warning string");
            logger.Information("Log.Logger => This is an information string");
            logger.Error("Log.Logger => This is an error string");
            logger.Fatal("Log.Logger => This is a fatal string");

            Container.GetTestClass.TestLogging();
        }
    }
}
