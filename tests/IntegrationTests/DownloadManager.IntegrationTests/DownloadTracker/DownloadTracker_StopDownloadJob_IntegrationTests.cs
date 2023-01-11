namespace DownloadManager.IntegrationTests.DownloadTracker;

[Collection("Sequential")]
public class DownloadTracker_StopDownloadJob_IntegrationTests : BaseIntegrationTests
{
    public DownloadTracker_StopDownloadJob_IntegrationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldStopDownloadJobAfterStartingForMovieAndEndWithStatusStopped_WhenGivenAValidDownloadTask()
    {
        // Arrange
        Seed = 4564;
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.MovieCount = 5;
            config.MovieDownloadTasksCount = 2;
        });
        SpinUpPlexServer();
        await CreateContainer(config =>
        {
            config.DownloadSpeedLimitInKib = 500;
            config.MockDownloadSubscriptions = new MockDownloadSubscriptions();
        });

        var plexMovieDownloadTask =
            Container.PlexRipperDbContext
                .DownloadTasks
                .FirstOrDefault(x => x.DownloadTaskType == DownloadTaskType.MovieData);
        plexMovieDownloadTask.ShouldNotBeNull();

        DownloadTask stoppedDownloadTask = null;
        var downloadTracker = Container.GetDownloadTracker;
        downloadTracker.DownloadTaskStopped.Subscribe(task => stoppedDownloadTask = task);

        // Act
        await Container.GetDownloadTracker.StartDownloadClient(plexMovieDownloadTask.Id);
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