using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace DownloadManager.IntegrationTests.DownloadTracker
{
    [Collection("Sequential")]
    public class DownloadTracker_StopDownloadJob_IntegrationTests : BaseIntegrationTests
    {
        public DownloadTracker_StopDownloadJob_IntegrationTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task ShouldStopDownloadJobAfterStartingForMovieAndEndWithStatusStopped_WhenGivenAValidDownloadTask()
        {
            // Arrange
            var config = new UnitTestDataConfig
            {
                Seed = 4564,
                MovieDownloadTasksCount = 2,
                DownloadSpeedLimit = 1000,
                MockServerConfig = new PlexMockServerConfig
                {
                    DownloadFileSizeInMb = 50,
                },
                MockDownloadSubscriptions = new MockDownloadSubscriptions(),
            };

            await CreateContainer(config);
            var plexMovieDownloadTask =
                Container.PlexRipperDbContext
                    .DownloadTasks
                    .FirstOrDefault(x => x.DownloadTaskType == DownloadTaskType.MovieData);
            plexMovieDownloadTask.ShouldNotBeNull();

            DownloadTask stoppedDownloadTask = null;
            var downloadTracker = Container.GetDownloadTracker;
            downloadTracker.DownloadTaskStopped.Subscribe(task => stoppedDownloadTask = task);

            // Act
            // ** We can't await otherwise the DownloadTask would have finished already before attempting to stop it
#pragma warning disable CS4014
            Container.GetDownloadTracker.StartDownloadClient(plexMovieDownloadTask.Id);
#pragma warning restore CS4014
            await Task.Delay(2000);
            var stopResult = await Container.GetDownloadTracker.StopDownloadClient(plexMovieDownloadTask.Id);

            // Assert
            stopResult.IsSuccess.ShouldBeTrue();
            stoppedDownloadTask.ShouldNotBeNull();
            stoppedDownloadTask.Id.ShouldBe(plexMovieDownloadTask.Id);
            stoppedDownloadTask.DownloadStatus.ShouldBe(DownloadStatus.Stopped);
            Container.GetDownloadTracker.IsDownloading(plexMovieDownloadTask.Id).ShouldBeFalse();
        }
    }
}