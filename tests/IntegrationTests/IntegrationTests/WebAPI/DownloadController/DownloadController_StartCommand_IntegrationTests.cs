using Data.Contracts;
using Microsoft.EntityFrameworkCore;
using PlexRipper.WebAPI.Common;
using PlexRipper.WebAPI.Common.FluentResult;

namespace IntegrationTests.WebAPI.DownloadController;

public class DownloadController_StartCommand_IntegrationTests : BaseIntegrationTests
{
    public DownloadController_StartCommand_IntegrationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldStartQueuedMovieDownloadTaskOnStartCommand_WhenNoTasksAreDownloading()
    {
        // Arrange
        Seed = 5594564;
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

        await CreateContainer();
        var downloadTasks = await DbContext.DownloadTaskMovie.ToListAsync();
        downloadTasks.Count.ShouldBe(10);
        var downloadTask = downloadTasks.First();

        // Act
        var response = await Container.GetAsync(ApiRoutes.Download.GetStartCommand(downloadTask.Id));
        var result = await response.Deserialize<ResultDTO>();
        await Container.SchedulerService.AwaitScheduler();
        await Task.Delay(2000);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var downloadTaskDb = await DbContext.DownloadTaskMovie.IncludeAll().SingleOrDefaultAsync(x => x.Id == downloadTask.Id);
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Completed);
        downloadTaskDb.Children.ShouldAllBe(x => x.DownloadStatus == DownloadStatus.Completed);
    }
}