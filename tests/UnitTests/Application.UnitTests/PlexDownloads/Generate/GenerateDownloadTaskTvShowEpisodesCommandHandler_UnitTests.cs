using Application.Contracts;
using Data.Contracts;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain.Validators;

namespace PlexRipper.Application.UnitTests;

public class DownloadTaskFactory_GenerateTvShowEpisodesDownloadTasksAsync_UnitTests : BaseUnitTest<GenerateDownloadTaskTvShowEpisodesCommandHandler>
{
    private DownloadTaskTvShowValidator validator = new();

    public DownloadTaskFactory_GenerateTvShowEpisodesDownloadTasksAsync_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldHaveFailedResult_WhenPlexTvShowsAreEmpty()
    {
        // Arrange
        await SetupDatabase();
        var tvShows = new List<DownloadMediaDTO> { };

        // Act
        var command = new GenerateDownloadTaskTvShowEpisodesCommand(tvShows);
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldGenerateValidTvShowDownloadTaskWithEpisodeDownloadTask_WhenNoDownloadTasksExist()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.TvShowCount = 5;
            config.TvShowSeasonCount = 5;
            config.TvShowEpisodeCount = 5;
        });

        var plexTvShows = await DbContext.PlexTvShows.IncludeAll().ToListAsync();
        var plexEpisodes = plexTvShows
            .SelectMany(x => x.Seasons)
            .SelectMany(x => x.Episodes)
            .ToList();
        var tvShows = new List<DownloadMediaDTO>
        {
            new()
            {
                Type = PlexMediaType.Episode,
                MediaIds = plexEpisodes.Select(x => x.Id).ToList(),
                PlexServerId = 1,
                PlexLibraryId = 1,
            },
        };

        // Act
        var command = new GenerateDownloadTaskTvShowEpisodesCommand(tvShows);
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue(result.ToString());
        var downloadTaskTvShows = await DbContext.DownloadTaskTvShow.IncludeAll().ToListAsync();
        downloadTaskTvShows.Count.ShouldBe(5);

        foreach (var downloadTaskMovie in downloadTaskTvShows)
        {
            downloadTaskMovie.Calculate();
            (await validator.ValidateAsync(downloadTaskMovie)).Errors.ShouldBeEmpty();
        }
    }

    //
    // [Fact]
    // public async Task ShouldGenerateValidEpisodeDownloadTask_WhenTvShowParentDownloadTaskAlreadyExist()
    // {
    //     // Arrange
    //     await SetupDatabase(config =>
    //     {
    //         config.PlexServerCount = 1;
    //         config.PlexLibraryCount = 1;
    //         config.TvShowCount = 5;
    //         config.TvShowSeasonCount = 2;
    //         config.TvShowEpisodeCount = 5;
    //     });
    //
    //     mock.AddMapper();
    //     var tvShows = await DbContext.PlexTvShows.IncludePlexLibrary().IncludePlexServer().IncludeEpisodes().ToListAsync();
    //     var tvShowDb = tvShows.Last();
    //     var episodeIds = new List<int> { tvShowDb.Seasons.First().Episodes.Last().Id };
    //
    //     var downloadTask = new DownloadTask()
    //     {
    //         Key = tvShowDb.Key,
    //         Title = tvShowDb.Title,
    //         FullTitle = tvShowDb.FullTitle,
    //         Year = tvShowDb.Year,
    //         MediaType = tvShowDb.Type,
    //         PlexServerId = tvShowDb.PlexServerId,
    //         PlexLibraryId = tvShowDb.PlexLibraryId,
    //         DownloadTaskType = DownloadTaskType.TvShow,
    //         DownloadStatus = DownloadStatus.Queued,
    //         DownloadFolderId = 1,
    //         DestinationFolderId = 1,
    //     };
    //     DbContext.DownloadTasks.AddRange(downloadTask);
    //     await DbContext.SaveChangesAsync();
    //     ResetDbContext();
    //
    //     // Act
    //     var result = await mock.Create<DownloadTaskFactory>().GenerateTvShowEpisodesDownloadTasksAsync(episodeIds);
    //
    //     // Assert
    //     result.IsSuccess.ShouldBeTrue();
    //     var tvShowDownloadTask = result.Value.First();
    //     tvShowDownloadTask.Id.ShouldBe(downloadTask.Id);
    //
    //     tvShowDownloadTask.Children.ShouldAllBe(x => x.Id == 0);
    //     tvShowDownloadTask.Children.SelectMany(x => x.Children).ToList().ShouldAllBe(x => x.Id == 0);
    //     tvShowDownloadTask.Children.ShouldAllBe(x => x.ParentId == tvShowDownloadTask.Id);
    // }
}