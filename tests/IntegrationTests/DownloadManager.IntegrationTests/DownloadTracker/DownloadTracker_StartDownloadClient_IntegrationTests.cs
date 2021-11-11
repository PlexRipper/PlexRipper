using System.Threading.Tasks;
using Logging;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace DownloadManager.IntegrationTests.DownloadTracker
{
    public class DownloadTracker_CreateClient
    {
        public DownloadTracker_CreateClient(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public async Task ShouldCreateDownloadClient_WhenGivenValidDownloadTask()
        {
            // Arrange
            var dbName = MockDatabase.GetMemoryDatabaseName();
            var container = new BaseContainer(false, dbName);
            var config = new FakeDataConfig
            {
                IncludeLibraries = true,
                PlexServerId = 1,
                PlexLibraryId = 1,
                LibraryType = PlexMediaType.Movie,
                DownloadTasksCount = 1,
            };
            var context = container.PlexRipperDbContext.AddPlexServers(config).AddDownloadTasks(config);

            // Act
            var startResult = await container.GetDownloadTracker.StartDownloadClient(2);

            // Assert
            startResult.IsSuccess.ShouldBeTrue();
        }
    }
}