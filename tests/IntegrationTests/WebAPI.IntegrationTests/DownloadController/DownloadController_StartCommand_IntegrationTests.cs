using Microsoft.EntityFrameworkCore;
using PlexRipper.WebAPI.Common;
using PlexRipper.WebAPI.Common.FluentResult;

namespace WebAPI.IntegrationTests.DownloadController;

[Collection("Sequential")]
public class DownloadController_StartCommand_IntegrationTests : BaseIntegrationTests
{
    public DownloadController_StartCommand_IntegrationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldStartQueuedMovieDownloadTaskOnStartCommand_WhenNoTasksAreDownloading()
    {
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

        // Arrange
        await CreateContainer(config => { config.MockDownloadSubscriptions = new MockDownloadSubscriptions(); });
        var downloadTasks = await Container.PlexRipperDbContext.DownloadTasks.ToListAsync();
        downloadTasks.Count.ShouldBe(10);

        // Act
        var response = await Container.GetAsync(ApiRoutes.Download.GetStartCommand(downloadTasks.First().Id));
        var result = await response.Deserialize<ResultDTO>();
        await Task.Delay(5000);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // TODO Add better success checks here
    }
}