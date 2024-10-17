using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;

namespace IntegrationTests.WebAPI.DownloadController;

public class DownloadControllerDownloadMediaIntegrationTests : BaseIntegrationTests
{
    public DownloadControllerDownloadMediaIntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldDownloadMultipleMovieDownloadTasks_WhenDownloadTasksAreCreated()
    {
        // Arrange
        var plexMovieCount = 3;

        using var container = await CreateContainer(
            231156,
            config =>
            {
                config.DownloadSpeedLimitInKib = 25000;
                config.PlexMockApiOptions = x =>
                {
                    x.MockServers.Add(new PlexMockServerConfig { DownloadFileSizeInMb = 50 });
                };
                config.DatabaseOptions = x =>
                {
                    x.PlexAccountCount = 1;
                    x.PlexServerCount = 1;
                    x.PlexLibraryCount = 1;
                    x.MovieCount = plexMovieCount;
                };
            }
        );

        var plexMovies = await container.DbContext.PlexMovies.ToListAsync();
        plexMovies.Count.ShouldBe(
            plexMovieCount,
            $"PlexMovies count should be 10 failed with database name: {container.DbContext.DatabaseName}"
        );

        var dtoList = new List<DownloadMediaDTO>()
        {
            new()
            {
                Type = PlexMediaType.Movie,
                MediaIds = plexMovies.Select(x => x.Id).ToList(),
                PlexServerId = plexMovies.First().PlexServerId,
                PlexLibraryId = plexMovies.First().PlexLibraryId,
            },
        };

        // Act
        var response = await container.ApiClient.POSTAsync<
            CreateDownloadTasksEndpoint,
            List<DownloadMediaDTO>,
            ResultDTO
        >(dtoList);
        response.Response.IsSuccessStatusCode.ShouldBeTrue();
        await Task.Delay(2000);
        await container.SchedulerService.AwaitScheduler();
        await Task.Delay(2000);

        // Assert
        var result = response.Result;
        result.IsSuccess.ShouldBeTrue();
        var downloadTasksDb = await container.DbContext.GetAllDownloadTasksByServerAsync();
        downloadTasksDb.ShouldNotBeNull();
        downloadTasksDb.ShouldNotBeEmpty();
        downloadTasksDb.Count.ShouldBe(plexMovieCount);
        downloadTasksDb.ShouldAllBe(x => x.DownloadStatus == DownloadStatus.Completed);
        downloadTasksDb.SelectMany(x => x.Children).ShouldAllBe(x => x.DownloadStatus == DownloadStatus.Completed);
    }
}
