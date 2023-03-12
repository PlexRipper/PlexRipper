using PlexRipper.Data.Common;
using Serilog.Events;

namespace IntegrationTests.DownloadManager.DownloadCommands;

[Collection("Sequential")]
public class DownloadCommands_PauseDownload_IntegrationTests : BaseIntegrationTests
{
    public DownloadCommands_PauseDownload_IntegrationTests(ITestOutputHelper output) : base(output, LogEventLevel.Verbose) { }

    [Fact]
    public async Task ShouldPauseADownloadTask_WhenDownloadTaskIsInProgressAndIsPaused()
    {
        // Arrange

        var serverUri = SpinUpPlexServer(config => { config.DownloadFileSizeInMb = 50; });
        await SetupDatabase(config =>
        {
            config.MockServerUris.Add(serverUri);
            config.PlexAccountCount = 1;
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.MovieCount = 1;
            config.MovieDownloadTasksCount = 1;
            config.DownloadFileSizeInMb = 50;
        });

        SetupMockPlexApi();

        await CreateContainer(config => config.DownloadSpeedLimitInKib = 5000);

        var downloadTask = DbContext
            .DownloadTasks
            .IncludeDownloadTasks()
            .FirstOrDefault();
        downloadTask.ShouldNotBeNull();
        var childDownloadTask = downloadTask.Children[0];

        // Act
        var startResult = await Container.GetDownloadCommands.StartDownloadTask(childDownloadTask.Id);
        await Task.Delay(2000);

        var pauseResult = await Container.GetDownloadCommands.PauseDownloadTask(childDownloadTask.Id);
        await Container.SchedulerService.AwaitScheduler();
        await Task.Delay(2000);

        // Assert

        startResult.IsSuccess.ShouldBeTrue();

        pauseResult.IsSuccess.ShouldBeTrue();

        var downloadTaskDb = DbContext
            .DownloadTasks
            .IncludeDownloadTasks()
            .FirstOrDefault(x => x.Id == childDownloadTask.Id);
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Paused);
    }
}