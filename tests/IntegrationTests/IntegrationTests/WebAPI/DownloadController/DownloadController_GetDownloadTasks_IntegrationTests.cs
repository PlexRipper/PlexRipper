using Application.Contracts;
using FastEndpoints;
using PlexRipper.Application;

namespace IntegrationTests.WebAPI.DownloadController;

public class DownloadControllerGetDownloadTasksIntegrationTests : BaseIntegrationTests
{
    public DownloadControllerGetDownloadTasksIntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldHaveAllDownloadTasksNested_WhenTasksAreAvailable()
    {
        // Arrange
        var tvShowDownloadTasksCount = 5;
        var tvShowSeasonDownloadTasksCount = 2;
        var tvShowEpisodeDownloadTasksCount = 3;

        using var container = await CreateContainer(
            45485864,
            config =>
            {
                config.DatabaseOptions = x =>
                {
                    x.PlexServerCount = 1;
                    x.PlexLibraryCount = 2;
                    x.TvShowCount = 5;
                    x.TvShowDownloadTasksCount = tvShowDownloadTasksCount;
                    x.TvShowSeasonDownloadTasksCount = tvShowSeasonDownloadTasksCount;
                    x.TvShowEpisodeDownloadTasksCount = tvShowEpisodeDownloadTasksCount;
                };
                config.PlexMockApiOptions = x =>
                {
                    x.MockServers.Add(new PlexMockServerConfig());
                };
            }
        );

        // Act
        var response = await container.ApiClient.GETAsync<
            GetAllDownloadTasksEndpoint,
            ResultDTO<List<ServerDownloadProgressDTO>>
        >();
        response.Response.IsSuccessStatusCode.ShouldBeTrue();

        // Assert
        var result = response.Result;
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

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
