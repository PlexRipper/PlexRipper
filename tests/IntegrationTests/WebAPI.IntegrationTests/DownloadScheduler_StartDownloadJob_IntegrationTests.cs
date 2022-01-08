using System;
using System.Linq;
using System.Threading.Tasks;
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

            await CreateContainer(config);
            var plexMovieDownloadTask =
                Container.PlexRipperDbContext
                    .DownloadTasks
                    .FirstOrDefault(x => x.DownloadTaskType == DownloadTaskType.MovieData);
            plexMovieDownloadTask.ShouldNotBeNull();

            // Act
            var startedDownloadTaskId = 0;
            Container.GetDownloadTracker.DownloadTaskStart.Subscribe(task => startedDownloadTaskId = task.Id);
            var startResult = await Container.DownloadScheduler.StartDownloadJob(plexMovieDownloadTask.Id);

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