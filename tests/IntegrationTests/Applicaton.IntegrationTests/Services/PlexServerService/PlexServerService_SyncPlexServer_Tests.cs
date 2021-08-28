using System.Linq;
using System.Threading.Tasks;
using Logging;
using PlexRipper.BaseTests;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace PlexRipper.Application.IntegrationTests.Services.PlexServerService
{
    public class PlexServerService_SyncPlexServer_IntegrationTests
    {
        private BaseContainer Container { get; }

        public PlexServerService_SyncPlexServer_IntegrationTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
            Container = new BaseContainer();
        }

        [Fact]
        public async Task SyncPlexServer_ShouldSyncCorrectly_WhenServerIsAvailable()
        {
            // Arrange
            var plexAccount = await Container.SetupTestAccount();
            var plexServers = await Container.GetPlexServerService.RetrieveAccessiblePlexServersAsync(plexAccount);
            plexServers.IsSuccess.ShouldBeTrue();

            // Act
            var syncResult = await Container.GetPlexServerService.SyncPlexServer(plexServers?.Value?.First().Id ?? 0);

            // Assert
            plexAccount.ShouldNotBeNull();
            syncResult.IsSuccess.ShouldBeTrue();
        }
    }
}