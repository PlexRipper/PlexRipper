using System.Linq;
using System.Threading.Tasks;
using Logging;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace DownloadManager.IntegrationTests.DownloadScheduler
{
    public class DownloadScheduler_StartDownloadJob_IntegrationTests
    {
        public DownloadScheduler_StartDownloadJob_IntegrationTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public async Task ShouldStartDownloadJobForMovie_WhenGivenAValidDownloadTask()
        {
            // Arrange
            var config = new UnitTestDataConfig
            {
                Seed = 4564,
                MovieDownloadTasksCount = 5,
                DownloadSpeedLimit = 2000,
                MockServerConfig = new PlexMockServerConfig
                {
                    DownloadFileSizeInMb = 50,
                },
            };

            var container = await BaseContainer.Create(config);
            var plexMovieDownloadTask =
                container.PlexRipperDbContext
                    .DownloadTasks
                    .FirstOrDefault(x => x.DownloadTaskType == DownloadTaskType.MovieData);
            plexMovieDownloadTask.ShouldNotBeNull();

            // Act
            var startResult = await container.DownloadScheduler.StartDownloadJob(plexMovieDownloadTask.Id);
            await Task.Delay(1000);
            var awaitTask = container.GetDownloadTracker.GetDownloadProcessTask(plexMovieDownloadTask.Id);
            awaitTask.IsSuccess.ShouldBeTrue();
            await awaitTask.Value;

            // Assert
            startResult.IsSuccess.ShouldBeTrue();
        }
    }
}