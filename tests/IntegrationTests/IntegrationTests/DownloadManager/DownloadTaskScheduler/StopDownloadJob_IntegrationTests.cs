using Data.Contracts;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.DownloadManager.DownloadTaskScheduler;

public class StopDownloadJobIntegrationTests : BaseIntegrationTests
{
    public StopDownloadJobIntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldStartAndStopDownloadJob_WhenDownloadTaskHasBeenStopped()
    {
        // Arrange
        using var container = await CreateContainer(
            45644875,
            config =>
            {
                config.DownloadSpeedLimitInKib = 5000;
                config.DatabaseOptions = x =>
                {
                    x.PlexAccountCount = 1;
                    x.PlexServerCount = 1;
                    x.PlexLibraryCount = 3;
                    x.MovieCount = 10;
                    x.MovieDownloadTasksCount = 1;
                    x.DownloadFileSizeInMb = 50;
                };
                config.PlexMockApiOptions = x =>
                {
                    x.MockServers = [new PlexMockServerConfig() { DownloadFileSizeInMb = 50 }];
                    x.SignInResponseIsValid = true;
                };
            }
        );

        var movieDownloadTasks = await container.DbContext.DownloadTaskMovie.Include(x => x.Children).ToListAsync();

        var childDownloadTask = movieDownloadTasks[0].Children[0];

        // Act
        var startResult = await container.DownloadTaskScheduler.StartDownloadTaskJob(childDownloadTask.ToKey());
        await Task.Delay(500);
        var stopResult = await container.DownloadTaskScheduler.StopDownloadTaskJob(childDownloadTask.ToKey());
        await container.SchedulerService.AwaitScheduler();

        // Assert
        startResult.IsSuccess.ShouldBeTrue(startResult.ToString());
        stopResult.IsSuccess.ShouldBeTrue(stopResult.ToString());
        var downloadTaskDb = await container.DbContext.GetDownloadTaskAsync(childDownloadTask.ToKey());
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Stopped);
    }
}
