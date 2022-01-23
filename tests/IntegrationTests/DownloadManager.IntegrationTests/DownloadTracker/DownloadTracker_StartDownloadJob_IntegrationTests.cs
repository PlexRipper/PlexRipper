using System;
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
    public class DownloadTracker_StartDownloadJob_IntegrationTests : BaseIntegrationTests
    {
        public DownloadTracker_StartDownloadJob_IntegrationTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task ShouldExecuteDownloadTaskForMovieAndEndWithDownloadFinished_WhenGivenAValidDownloadTask()
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

            await CreateContainer(config);
            var plexMovieDownloadTask =
                Container.PlexRipperDbContext
                    .DownloadTasks
                    .FirstOrDefault(x => x.DownloadTaskType == DownloadTaskType.MovieData);
            plexMovieDownloadTask.ShouldNotBeNull();

            DownloadTask finishedDownloadTask = null;
            var downloadTracker = Container.GetDownloadTracker;
            downloadTracker.DownloadTaskFinished.Subscribe(task => finishedDownloadTask = task);

            // Act
            var startResult = await downloadTracker.StartDownloadClient(plexMovieDownloadTask.Id);
            await Task.Delay(2000);
            await downloadTracker.DownloadProcessTask;
            await Task.Delay(2000);

            // Assert
            startResult.IsSuccess.ShouldBeTrue();
            finishedDownloadTask.ShouldNotBeNull();
            finishedDownloadTask.Id.ShouldBe(plexMovieDownloadTask.Id);
            finishedDownloadTask.DownloadStatus.ShouldBe(DownloadStatus.DownloadFinished);
        }
    }
}