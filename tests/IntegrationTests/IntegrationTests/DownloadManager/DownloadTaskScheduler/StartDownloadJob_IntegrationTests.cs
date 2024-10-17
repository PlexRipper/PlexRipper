using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.DownloadManager.DownloadTaskScheduler;

public class StartDownloadJobIntegrationTests : BaseIntegrationTests
{
    public StartDownloadJobIntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldSendOutDownloadTaskUpdates_WhenDownloadTaskIsInProgress()
    {
        // Arrange
        using var container = await CreateContainer(
            68,
            config =>
            {
                config.DownloadSpeedLimitInKib = 5000;
                config.DatabaseOptions = x =>
                {
                    x.PlexAccountCount = 1;
                    x.PlexServerCount = 1;
                    x.PlexLibraryCount = 3;
                    x.MovieCount = 1;
                    x.MovieDownloadTasksCount = 1;
                    x.DownloadFileSizeInMb = 50;
                };
                config.PlexMockApiOptions = x =>
                {
                    x.MockServers.Add(new PlexMockServerConfig { DownloadFileSizeInMb = 50 });
                };
            }
        );
        var movieDownloadTasks = await container.DbContext.DownloadTaskMovie.Include(x => x.Children).ToListAsync();
        var movieFileDownloadTask = movieDownloadTasks[0].Children[0];

        // Act
        var startResult = await container.DownloadTaskScheduler.StartDownloadTaskJob(movieFileDownloadTask.ToKey());
        await container.SchedulerService.AwaitScheduler();

        // Assert
        startResult.IsSuccess.ShouldBeTrue();
        container.MockSignalRService.ServerDownloadProgressList.Count.ShouldBeGreaterThan(10);
    }
}
