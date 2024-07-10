using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using Serilog.Events;

namespace IntegrationTests.WebAPI.DownloadController;

public class DownloadController_DownloadMedia_IntegrationTests : BaseIntegrationTests
{
    public DownloadController_DownloadMedia_IntegrationTests(ITestOutputHelper output) : base(output, LogEventLevel.Verbose) { }

    [Fact]
    public async Task ShouldDownloadMultipleMovieDownloadTasks_WhenDownloadTasksAreCreated()
    {
        // Arrange
        var serverUri = SpinUpPlexServer(config => { config.DownloadFileSizeInMb = 50; });
        var plexMovieCount = 3;
        await SetupDatabase(config =>
        {
            config.MockServerUris.Add(serverUri);
            config.PlexAccountCount = 1;
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.MovieCount = plexMovieCount;
        });

        await CreateContainer(config => config.DownloadSpeedLimitInKib = 25000);
        var plexMovies = await DbContext.PlexMovies.ToListAsync();
        plexMovies.Count.ShouldBe(plexMovieCount, $"PlexMovies count should be 10 failed with database name: {DatabaseName}");

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
        var response = await Container.ApiClient.POSTAsync<CreateDownloadTasksEndpoint, List<DownloadMediaDTO>, ResultDTO>(dtoList);
        response.Response.IsSuccessStatusCode.ShouldBeTrue();
        await Task.Delay(2000);
        await Container.SchedulerService.AwaitScheduler();
        await Task.Delay(2000);

        // Assert
        var result = response.Result;
        result.IsSuccess.ShouldBeTrue();
        var downloadTasksDb = await DbContext.GetAllDownloadTasksByServerAsync();
        downloadTasksDb.ShouldNotBeNull();
        downloadTasksDb.ShouldNotBeEmpty();
        downloadTasksDb.Count.ShouldBe(plexMovieCount);
        downloadTasksDb.ShouldAllBe(x => x.DownloadStatus == DownloadStatus.Completed);
        downloadTasksDb.SelectMany(x => x.Children).ShouldAllBe(x => x.DownloadStatus == DownloadStatus.Completed);
    }
}