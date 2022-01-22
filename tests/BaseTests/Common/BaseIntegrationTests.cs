using System;
using System.Threading;
using System.Threading.Tasks;
using Logging;
using Xunit;
using Xunit.Abstractions;

namespace PlexRipper.BaseTests
{
    public class BaseIntegrationTests : IAsyncLifetime, IAsyncDisposable
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

        public async Task DisposeAsync() { }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            Log.Fatal("Container disposed");
            await Container.Boot.StopAsync(CancellationToken.None);
            Container?.Dispose();
        }
    }
}