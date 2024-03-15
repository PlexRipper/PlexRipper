using PlexRipper.WebAPI;
using PlexRipper.WebAPI.Common;

namespace IntegrationTests.WebAPI.DownloadController;

public class DownloadController_GetDownloadTasks_IntegrationTests : BaseIntegrationTests
{
    public DownloadController_GetDownloadTasks_IntegrationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldHaveAllDownloadTasksNested_WhenTasksAreAvailable()
    {
        // Arrange
        Seed = 45485864;

        var tvShowDownloadTasksCount = 5;
        var tvShowSeasonDownloadTasksCount = 2;
        var tvShowEpisodeDownloadTasksCount = 3;
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 2;
            config.TvShowCount = 5;
            config.TvShowDownloadTasksCount = tvShowDownloadTasksCount;
            config.TvShowSeasonDownloadTasksCount = tvShowSeasonDownloadTasksCount;
            config.TvShowEpisodeDownloadTasksCount = tvShowEpisodeDownloadTasksCount;
        });

        await CreateContainer();

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