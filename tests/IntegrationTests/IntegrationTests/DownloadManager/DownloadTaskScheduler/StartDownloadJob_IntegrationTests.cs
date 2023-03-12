using PlexRipper.Data.Common;

namespace IntegrationTests.DownloadManager.DownloadTaskScheduler;


public class StartDownloadJob_IntegrationTests : BaseIntegrationTests
{
    public StartDownloadJob_IntegrationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldSendOutDownloadTaskUpdates_WhenDownloadTaskIsInProgress()
    {
        // Arrange

        var serverUri = SpinUpPlexServer(config => { config.DownloadFileSizeInMb = 50; });
        await SetupDatabase(config =>
        {
            config.MockServerUris.Add(serverUri);
            config.PlexAccountCount = 1;
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 3;
            config.MovieCount = 1;
            config.MovieDownloadTasksCount = 1;
            config.DownloadFileSizeInMb = 50;
        });

        SetupMockPlexApi();

        await CreateContainer(config => { config.DownloadSpeedLimitInKib = 5000; });

        var downloadTask = Container.PlexRipperDbContext
            .DownloadTasks
            .IncludeDownloadTasks()
            .FirstOrDefault();
        downloadTask.ShouldNotBeNull();
        var childDownloadTask = downloadTask.Children[0];

        // Act
        var startResult = await Container.DownloadTaskScheduler.StartDownloadTaskJob(childDownloadTask.Id, childDownloadTask.PlexServerId);
        await Container.SchedulerService.AwaitScheduler();

        // Assert
        startResult.IsSuccess.ShouldBeTrue();
        Container.MockSignalRService.ServerDownloadProgressList.Count.ShouldBeGreaterThan(10);
    }
}