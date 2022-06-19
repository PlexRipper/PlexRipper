using System;
using System.Linq;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using PlexRipper.DownloadManager;
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
            await CreateContainer(config =>
            {
                config.Seed = 4564;
                config.MovieDownloadTasksCount = 2;
                config.DownloadSpeedLimit = 2000;
                config.MockDownloadSubscriptions = new MockDownloadSubscriptions();
                config.MockServerConfig = new PlexMockServerConfig
                {
                    DownloadFileSizeInMb = 50,
                };
            });
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