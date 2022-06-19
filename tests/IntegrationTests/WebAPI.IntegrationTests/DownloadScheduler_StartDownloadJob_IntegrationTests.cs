using System;
using System.Linq;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace WebAPI.IntegrationTests
{
    [Collection("Sequential")]
    public class DownloadScheduler_StartDownloadJob_IntegrationTests : BaseIntegrationTests
    {
        public DownloadScheduler_StartDownloadJob_IntegrationTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task ShouldStartDownloadJobForMovie_WhenGivenAValidDownloadTask()
        {
            // Arrange
            await CreateContainer(config =>
            {
                config.Seed = 4564;
                config.MovieDownloadTasksCount = 5;
                config.DownloadSpeedLimit = 2000;
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

            // Act
            var startedDownloadTaskId = 0;
            Container.GetDownloadTracker.DownloadTaskStart.Subscribe(task => startedDownloadTaskId = task.Id);
            var startResult = await Container.GetDownloadTracker.StartDownloadClient(plexMovieDownloadTask.Id);

            // TODO Check if this test is needed
            // while (startedDownloadTaskId == 0)
            // {
            //     await Task.Delay(2000);
            //     Log.Debug($"{nameof(startedDownloadTaskId)} is still 0, continue waiting");
            // }

            // Assert
            startResult.IsSuccess.ShouldBeTrue();
        }
    }
}