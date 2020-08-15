using PlexRipper.BaseTests;
using PlexRipper.Domain;
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

            Log.Verbose("This is a verbose string");
            Log.Debug("This is a debug string");
            Log.Warning("This is a warning string");
            Log.Information("This is an information string");
            Log.Error("This is an error string");
            Log.Fatal("This is a fatal string");
        }
    }
}
