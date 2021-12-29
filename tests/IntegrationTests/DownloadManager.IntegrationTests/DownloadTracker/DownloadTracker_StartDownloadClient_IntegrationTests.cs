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
            var config = new UnitTestDataConfig
            {
                MemoryDbName = MockDatabase.GetMemoryDatabaseName(),
                IncludeLibraries = true,
                LibraryType = PlexMediaType.Movie,
                DownloadTasksCount = 1,
            };
            var container = await BaseContainer.Create();

            // Act
            var startResult = await container.GetDownloadTracker.StartDownloadClient(2);

            // Assert
            startResult.IsSuccess.ShouldBeTrue();
        }
    }
}