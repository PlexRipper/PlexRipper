using Data.Contracts;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.BaseTests.Asserts;
using PlexRipper.DownloadManager;

namespace DownloadManager.UnitTests;

public class DownloadTaskFactory_GenerateTvShowEpisodesDownloadTasksAsync_UnitTests : BaseUnitTest<DownloadTaskFactory>
{
    public DownloadTaskFactory_GenerateTvShowEpisodesDownloadTasksAsync_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldHaveFailedResult_WhenPlexTvShowsAreEmpty()
    {
        // Arrange
        await SetupDatabase();
        var tvShowIds = new List<int>();

        // Act
        var result = await _sut.GenerateTvShowEpisodesDownloadTasksAsync(tvShowIds);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldGenerateValidTvShowDownloadTaskWithEpisodeDownloadTask_WhenNoDownloadTasksExist()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.TvShowCount = 5;
            config.TvShowSeasonCount = 2;
            config.TvShowEpisodeCount = 5;
            config.TvShowDownloadTasksCount = 5;
        });

        var tvShows = await DbContext.PlexTvShows.IncludeAll().ToListAsync();

        mock.AddMapper();
        mock.SetupMediator(It.IsAny<GetPlexTvShowByIdQuery>)
            .ReturnsAsync((GetPlexTvShowByIdQuery query, CancellationToken _) => Result.Ok(tvShows.Find(x => x.Id == query.PlexTvShowId)));

        mock.SetupMediator(It.IsAny<GetPlexTvShowEpisodeByIdQuery>)
            .ReturnsAsync((GetPlexTvShowEpisodeByIdQuery query, CancellationToken _) =>
                Result.Ok(DbContext.PlexTvShowEpisodes.IncludeAll().FirstOrDefault(x => x.Id == query.Id)));

        var tvShowDb = tvShows.Last();
        var episodeIds = new List<int> { tvShowDb.Seasons.First().Episodes.Last().Id };

        // Act
        var result = await _sut.GenerateTvShowEpisodesDownloadTasksAsync(episodeIds);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var tvShowDownloadTask = result.Value.First();
        tvShowDownloadTask.Id.ShouldBe(0);
        tvShowDownloadTask.Children.ShouldAllBe(x => x.Id == 0);
        tvShowDownloadTask.Children.SelectMany(x => x.Children).ToList().ShouldAllBe(x => x.Id == 0);
        ShouldDownloadTask.ShouldTvShow(tvShowDownloadTask, tvShowDb);
    }

    [Fact]
    public async Task ShouldGenerateValidEpisodeDownloadTask_WhenTvShowParentDownloadTaskAlreadyExist()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.TvShowCount = 5;
            config.TvShowSeasonCount = 2;
            config.TvShowEpisodeCount = 5;
            config.TvShowDownloadTasksCount = 5;
        });

        var tvShows = await DbContext.PlexTvShows.IncludeAll().ToListAsync();
        var tvShowDb = tvShows.Last();
        var episodeIds = new List<int> { tvShowDb.Seasons.First().Episodes.Last().Id };

        mock.AddMapper();

        mock.SetupMediator(It.IsAny<GetPlexTvShowEpisodeByIdQuery>, true)
            .ReturnsAsync((GetPlexTvShowEpisodeByIdQuery query, CancellationToken _) =>
                Result.Ok(DbContext.PlexTvShowEpisodes.IncludeAll().FirstOrDefault(x => x.Id == query.Id)));

        // Act
        var result = await _sut.GenerateTvShowEpisodesDownloadTasksAsync(episodeIds);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var tvShowDownloadTask = result.Value.First();
        tvShowDownloadTask.Id.ShouldBe(999);

        mock.VerifyMediator(It.IsAny<GetPlexTvShowEpisodeByIdQuery>, Times.Once);

        tvShowDownloadTask.Children.ShouldAllBe(x => x.Id == 0);
        tvShowDownloadTask.Children.SelectMany(x => x.Children).ToList().ShouldAllBe(x => x.Id == 0);
        tvShowDownloadTask.Children.ShouldAllBe(x => x.ParentId == tvShowDownloadTask.Id);
        ShouldDownloadTask.ShouldTvShow(tvShowDownloadTask, tvShowDb);
    }
}