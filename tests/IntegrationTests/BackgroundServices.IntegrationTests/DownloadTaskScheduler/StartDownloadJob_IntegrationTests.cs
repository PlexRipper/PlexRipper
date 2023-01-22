using System.Linq;
using System.Threading.Tasks;
using PlexRipper.Data.Common;

namespace BackgroundServices.IntegrationTests.DownloadTaskScheduler;

[Collection("Sequential")]
public class StartDownloadJob_IntegrationTests : BaseIntegrationTests
{
    public StartDownloadJob_IntegrationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldStartAndFinishDownloadJob_WhenDownloadTaskHasFinishedDownloading()
    {
        // Arrange
        Seed = 4564;
        var serverUri = SpinUpPlexServer(config => { config.DownloadFileSizeInMb = 50; });
        await SetupDatabase(config =>
        {
            config.MockServerUris.Add(serverUri);
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 3;
            config.MovieCount = 10;
            config.MovieDownloadTasksCount = 5;
            config.DownloadFileSizeInMb = 50;
        });

        SetupMockPlexApi();

        await CreateContainer(config => { config.DownloadSpeedLimitInKib = 5000; });

        var downloadTask = DbContext
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
        var downloadTaskDb = DbContext.DownloadTasks
            .IncludeDownloadTasks()
            .FirstOrDefault(x => x.Id == childDownloadTask.Id);
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Completed);
    }
}