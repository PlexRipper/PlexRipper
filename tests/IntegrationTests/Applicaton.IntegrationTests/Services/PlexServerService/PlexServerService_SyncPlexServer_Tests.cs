using System.Threading.Tasks;
using PlexRipper.BaseTests;
using Xunit;
using Xunit.Abstractions;

namespace PlexRipper.Application.IntegrationTests.Services.PlexServerService
{
    public class PlexServerService_SyncPlexServer_IntegrationTests
    {
        private BaseContainer Container { get; }

        public PlexServerService_SyncPlexServer_IntegrationTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.SetupLogging(output);
            Container = new BaseContainer();
        }

        [Fact]
        public async Task SyncPlexServer_ShouldSyncCorrectly_WhenServerIsAvailable()
        {
            // Arrange
           // await Container.SetupTestAccount();

            // Act
           // var result = Container.GetPlexServerService.SyncPlexServer();

            // Assert
        }
    }
}