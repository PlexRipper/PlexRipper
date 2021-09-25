using Logging;
using PlexRipper.BaseTests;
using Xunit.Abstractions;

namespace PlexRipper.Application.IntegrationTests
{
    public class PlexDownloadServiceIntegrationTests
    {
        private BaseContainer Container { get; }

        public PlexDownloadServiceIntegrationTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
            Container = new BaseContainer();
        }
    }
}