using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;
using PlexRipper.WebAPI.Common;
using PlexRipper.WebAPI.Common.FluentResult;

namespace WebAPI.IntegrationTests.DownloadController;

[Collection("Sequential")]
public class DownloadController_DownloadMedia_IntegrationTests : BaseIntegrationTests
{
    public DownloadController_DownloadMedia_IntegrationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldDownloadMultipleMovieDownloadTasks_WhenDownloadTasksAreCreated()
    {
        Seed = 45634;
        var serverUri = SpinUpPlexServer(config => { config.DownloadFileSizeInMb = 50; });

        await SetupDatabase(config =>
        {
            config.MockServerUris.Add(serverUri);
            config.PlexAccountCount = 1;
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.MovieCount = 10;
        });

        // Arrange
        await CreateContainer();
        var plexMovies = await Container.PlexRipperDbContext.PlexMovies.ToListAsync();
        plexMovies.Count.ShouldBe(10);

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
        downloadTasksDb.Count.ShouldBe(10);
        downloadTasksDb.ShouldAllBe(x => x.DownloadStatus == DownloadStatus.DownloadFinished);

        foreach (var downloadTaskDb in downloadTasksDb)
            downloadTaskDb.Children.ShouldAllBe(y => y.DownloadStatus == DownloadStatus.DownloadFinished);
    }
}