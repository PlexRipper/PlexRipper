using PlexRipper.WebAPI.Common;
using PlexRipper.WebAPI.SignalR.Common;

namespace WebAPI.IntegrationTests.DownloadController;

[Collection("Sequential")]
public class DownloadController_GetDownloadTasks_IntegrationTests : BaseIntegrationTests
{
    public DownloadController_GetDownloadTasks_IntegrationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldHaveAllDownloadTasksNested_WhenTasksAreAvailable()
    {
        // Arrange
        var tvShowDownloadTasksCount = 5;
        var tvShowSeasonDownloadTasksCount = 2;
        var tvShowEpisodeDownloadTasksCount = 3;

        await SetupDatabase(config =>
        {
            config.TvShowDownloadTasksCount = tvShowDownloadTasksCount;
            config.TvShowSeasonDownloadTasksCount = tvShowSeasonDownloadTasksCount;
            config.TvShowEpisodeDownloadTasksCount = tvShowEpisodeDownloadTasksCount;
        });

        await CreateContainer(config => { config.Seed = 4564; });

        // Act
        var response = await Container.ApiClient.GetAsync(ApiRoutes.Download.GetDownloadTasks);
        var result = await response.Deserialize<List<ServerDownloadProgressDTO>>();

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        result.IsSuccess.ShouldBeTrue();
        var plexServer = result.Value.First();
        plexServer.ShouldNotBeNull();
        plexServer.Downloads.Count.ShouldBe(tvShowDownloadTasksCount);
        foreach (var downloadProgressDto in plexServer.Downloads)
        {
            downloadProgressDto.Children.Count.ShouldBe(tvShowSeasonDownloadTasksCount);
            foreach (var child in downloadProgressDto.Children)
                child.Children.Count.ShouldBe(tvShowEpisodeDownloadTasksCount);
        }
    }
}