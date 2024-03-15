using Data.Contracts;
using DownloadManager.Contracts;
using Microsoft.EntityFrameworkCore;
using PlexRipper.WebAPI.Common;
using PlexRipper.WebAPI.Common.FluentResult;
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
        var response = await Container.PostAsync(ApiRoutes.Download.PostDownloadMedia, dtoList);
        var result = await response.Deserialize<ResultDTO>();
        await Task.Delay(2000);
        await Container.SchedulerService.AwaitScheduler();
        await Task.Delay(2000);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var downloadTasksDb = await DbContext.GetAllDownloadTasksAsync();
        downloadTasksDb.ShouldNotBeNull();
        downloadTasksDb.ShouldNotBeEmpty();
        downloadTasksDb.Count.ShouldBe(plexMovieCount);
        downloadTasksDb.ShouldAllBe(x => x.DownloadStatus == DownloadStatus.Completed);
        downloadTasksDb.SelectMany(x => x.Children).ShouldAllBe(x => x.DownloadStatus == DownloadStatus.Completed);
    }
}