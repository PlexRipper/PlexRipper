using System.Threading.Tasks;
using Logging;
using Xunit;
using Xunit.Abstractions;

namespace PlexRipper.BaseTests
{
    [Collection("Sequential")]
    public class BaseIntegrationTests : IAsyncLifetime
    {
        protected BaseContainer Container;

        protected BaseIntegrationTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        protected async Task CreateContainer(UnitTestDataConfig config = null)
        {
            Container = await BaseContainer.Create(config);
        }

        public async Task InitializeAsync()
        {
            Log.Information("Initialize Integration Test");
        }

        public async Task DisposeAsync()
        {
            // Log.Fatal("Container disposed");
            // Container.Dispose();

        }
    }
}