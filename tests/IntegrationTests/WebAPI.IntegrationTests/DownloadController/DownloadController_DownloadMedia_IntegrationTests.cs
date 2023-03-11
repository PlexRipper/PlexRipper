using Application.Contracts;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;
using PlexRipper.WebAPI.Common;
using PlexRipper.WebAPI.Common.FluentResult;
using Serilog.Events;

namespace WebAPI.IntegrationTests.DownloadController;

[Collection("Sequential")]
public class DownloadController_DownloadMedia_IntegrationTests : BaseIntegrationTests
{
    public DownloadController_DownloadMedia_IntegrationTests(ITestOutputHelper output) : base(output, LogEventLevel.Verbose) { }

    [Fact]
    public async Task ShouldDownloadMultipleMovieDownloadTasks_WhenDownloadTasksAreCreated()
    {
        var serverUri = SpinUpPlexServer(config => { config.DownloadFileSizeInMb = 50; });
        var plexMovieCount = 5;
        await SetupDatabase(config =>
        {
            config.MockServerUris.Add(serverUri);
            config.PlexAccountCount = 1;
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.MovieCount = plexMovieCount;
        });

        // Arrange
        await CreateContainer(config => { config.DownloadSpeedLimitInKib = 5000; });
        var plexMovies = await DbContext.PlexMovies.ToListAsync();
        plexMovies.Count.ShouldBe(plexMovieCount, $"PlexMovies count should be 10 failed with database name: {DatabaseName}");

        var dtoList = new List<DownloadMediaDTO>()
        {
            new()
            {
                Type = PlexMediaType.Movie,
                MediaIds = plexMovies.Select(x => x.Id).ToList(),
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
        var downloadTasksDb = await DbContext.DownloadTasks
            .IncludeDownloadTasks()
            .ToListAsync();
        downloadTasksDb.ShouldNotBeNull();
        downloadTasksDb.ShouldNotBeEmpty();
        downloadTasksDb.Count.ShouldBe(plexMovieCount);
        downloadTasksDb.ShouldAllBe(x => x.DownloadStatus == DownloadStatus.Completed);

        foreach (var downloadTaskDb in downloadTasksDb)
            downloadTaskDb.Children.ShouldAllBe(y => y.DownloadStatus == DownloadStatus.Completed);
    }
}