using System;
using System.Linq;
using System.Threading.Tasks;
using Logging;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace DownloadManager.IntegrationTests.DownloadTracker
{
    public class DownloadTracker_StartDownloadJob_IntegrationTests
    {
        public DownloadTracker_StartDownloadJob_IntegrationTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public async Task ShouldStartDownloadJobForMovieAndEndWithDownloadFinished_WhenGivenAValidDownloadTask()
        {
            // Arrange
            var config = new UnitTestDataConfig
            {
                Seed = 4564,
                MovieDownloadTasksCount = 2,
                DownloadSpeedLimit = 2000,
                MockServerConfig = new PlexMockServerConfig
                {
                    DownloadFileSizeInMb = 50,
                },
                MockDownloadSubscriptions = new MockDownloadSubscriptions(),
            };

            var container = await BaseContainer.Create(config);
            var plexMovieDownloadTask =
                container.PlexRipperDbContext
                    .DownloadTasks
                    .FirstOrDefault(x => x.DownloadTaskType == DownloadTaskType.MovieData);
            plexMovieDownloadTask.ShouldNotBeNull();

            DownloadTask finishedDownloadTask = null;
            var downloadTracker = container.GetDownloadTracker;
            downloadTracker.DownloadTaskFinished.Subscribe(task => finishedDownloadTask = task);

            // Act
            var startResult = await container.GetDownloadTracker.StartDownloadClient(plexMovieDownloadTask.Id);
            await Task.Delay(2000);

            // Assert
            startResult.IsSuccess.ShouldBeTrue();
            finishedDownloadTask.ShouldNotBeNull();
            finishedDownloadTask.Id.ShouldBe(plexMovieDownloadTask.Id);
            finishedDownloadTask.DownloadStatus.ShouldBe(DownloadStatus.DownloadFinished);
        }
    }
}