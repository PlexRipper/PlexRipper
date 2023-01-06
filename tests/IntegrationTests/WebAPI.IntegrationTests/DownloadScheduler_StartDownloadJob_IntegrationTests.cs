namespace WebAPI.IntegrationTests;

[Collection("Sequential")]
public class DownloadScheduler_StartDownloadJob_IntegrationTests : BaseIntegrationTests
{
    public DownloadScheduler_StartDownloadJob_IntegrationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldStartDownloadJobForMovie_WhenGivenAValidDownloadTask()
    {
        // Arrange
        Seed = 4564;
        var serverUri = SpinUpPlexServer(config => { config.DownloadFileSizeInMb = 50; });

        await SetupDatabase(config =>
        {
            config.MockServerUris.Add(serverUri);
            config.PlexAccountCount = 1;
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 2;
            config.MovieCount = 10;
            config.MovieDownloadTasksCount = 5;
        });

        await CreateContainer(config =>
        {
            config.DownloadSpeedLimit = 2000;
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
        var loop = 0;
        while (startedDownloadTaskId == 0 && loop < 20)
        {
            await Task.Delay(2000);
            Log.Debug($"{nameof(startedDownloadTaskId)} is still 0, continue waiting");
            loop++;
        }

        // Assert
        startResult.IsSuccess.ShouldBeTrue();
        startedDownloadTaskId.ShouldBeGreaterThan(0);
    }
}