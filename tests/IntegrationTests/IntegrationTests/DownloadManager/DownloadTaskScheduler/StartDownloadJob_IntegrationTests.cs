using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.DownloadManager.DownloadTaskScheduler;

public class StartDownloadJob_IntegrationTests : BaseIntegrationTests
{
    public StartDownloadJob_IntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldSendOutDownloadTaskUpdates_WhenDownloadTaskIsInProgress()
    {
        // Arrange
        using var Container = await CreateContainer(config =>
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
        });
        var movieDownloadTasks = await Container.DbContext.DownloadTaskMovie.Include(x => x.Children).ToListAsync();
        var movieFileDownloadTask = movieDownloadTasks[0].Children[0];

        // Act
        var startResult = await Container.DownloadTaskScheduler.StartDownloadTaskJob(movieFileDownloadTask.ToKey());
        await Container.SchedulerService.AwaitScheduler();

        // Assert
        startResult.IsSuccess.ShouldBeTrue();
        Container.MockSignalRService.ServerDownloadProgressList.Count.ShouldBeGreaterThan(10);
    }
}
