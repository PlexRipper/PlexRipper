using PlexRipper.DownloadManager;

namespace DownloadManager.UnitTests;

public class DownloadTaskFactory_GenerateTvShowEpisodesDownloadTasksAsync_UnitTests : BaseUnitTest
{
    public DownloadTaskFactory_GenerateTvShowEpisodesDownloadTasksAsync_UnitTests(ITestOutputHelper output) : base(output) { }

    // [Fact]
    // public async Task ShouldHaveFailedResult_WhenPlexTvShowsAreEmpty()
    // {
    //     // Arrange
    //     await SetupDatabase();
    //     var tvShowIds = new List<int>();
    //
    //     // Act
    //     var result = await _sut.GenerateTvShowEpisodesDownloadTasksAsync(tvShowIds);
    //
    //     // Assert
    //     result.IsFailed.ShouldBeTrue();
    // }
    //
    // [Fact]
    // public async Task ShouldGenerateValidTvShowDownloadTaskWithEpisodeDownloadTask_WhenNoDownloadTasksExist()
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
    //     var tvShows = await DbContext.PlexTvShows.IncludeAll().ToListAsync();
    //
    //     mock.AddMapper();
    //     mock.SetupMediator(It.IsAny<GetPlexTvShowByIdQuery>)
    //         .ReturnsAsync((GetPlexTvShowByIdQuery query, CancellationToken _) => Result.Ok(tvShows.Find(x => x.Id == query.PlexTvShowId)));
    //
    //     mock.SetupMediator(It.IsAny<GetPlexTvShowEpisodeByIdQuery>)
    //         .ReturnsAsync((GetPlexTvShowEpisodeByIdQuery query, CancellationToken _) =>
    //             Result.Ok(DbContext.PlexTvShowEpisodes.IncludeAll().FirstOrDefault(x => x.Id == query.Id)));
    //
    //     var tvShowDb = tvShows.Last();
    //     var episodeIds = new List<int> { tvShowDb.Seasons.First().Episodes.Last().Id };
    //
    //     // Act
    //     var result = await _sut.GenerateTvShowEpisodesDownloadTasksAsync(episodeIds);
    //
    //     // Assert
    //     result.IsSuccess.ShouldBeTrue();
    //     var tvShowDownloadTask = result.Value.First();
    //     tvShowDownloadTask.Id.ShouldBe(0);
    //     tvShowDownloadTask.Children.ShouldAllBe(x => x.Id == 0);
    //     tvShowDownloadTask.Children.SelectMany(x => x.Children).ToList().ShouldAllBe(x => x.Id == 0);
    //     ShouldDownloadTask.ShouldTvShow(tvShowDownloadTask, tvShowDb);
    // }
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