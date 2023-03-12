using Data.Contracts;
using Microsoft.EntityFrameworkCore;
using PlexRipper.BaseTests.Asserts;
using PlexRipper.Data.Common;
using PlexRipper.DownloadManager;
using PlexRipper.WebAPI;

namespace DownloadManager.UnitTests;

public class DownloadTaskFactory_GenerateTvShowEpisodesDownloadTasksAsync_UnitTests : BaseUnitTest<DownloadTaskFactory>
{
    public DownloadTaskFactory_GenerateTvShowEpisodesDownloadTasksAsync_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldHaveFailedResult_WhenPlexTvShowsAreEmpty()
    {
        // Arrange
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
            config.TvShowCount = 10;
            config.TvShowDownloadTasksCount = 5;
        });

        var tvShows = await DbContext.PlexTvShows.IncludeAll().ToListAsync();

        mock.AddMapper();
        mock.SetupMediator(It.IsAny<GetPlexTvShowByIdQuery>)
            .ReturnsAsync((GetPlexTvShowByIdQuery query, CancellationToken _) => Result.Ok(tvShows.Find(x => x.Id == query.Id)));

        mock.SetupMediator(It.IsAny<GetPlexTvShowEpisodeByIdQuery>)
            .ReturnsAsync((GetPlexTvShowEpisodeByIdQuery query, CancellationToken _) =>
                Result.Ok(DbContext.PlexTvShowEpisodes.IncludeAll().FirstOrDefault(x => x.Id == query.Id)));

        mock.SetupMediator(It.IsAny<GetDownloadTaskByMediaKeyQuery>).ReturnsAsync(Result.Fail(""));

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
            config.TvShowCount = 10;
            config.TvShowDownloadTasksCount = 5;
        });

        var tvShows = await DbContext.PlexTvShows.IncludeAll().ToListAsync();
        var tvShowDb = tvShows.Last();
        var episodeIds = new List<int> { tvShowDb.Seasons.First().Episodes.Last().Id };

        mock.AddMapper();

        mock.SetupMediator(It.IsAny<GetPlexTvShowEpisodeByIdQuery>, true)
            .ReturnsAsync((GetPlexTvShowEpisodeByIdQuery query, CancellationToken _) =>
                Result.Ok(DbContext.PlexTvShowEpisodes.IncludeAll().FirstOrDefault(x => x.Id == query.Id)));

        mock.SetupMediator(It.IsAny<GetDownloadTaskByMediaKeyQuery>, true)
            .ReturnsAsync((GetDownloadTaskByMediaKeyQuery query, CancellationToken _) =>
            {
                // We create the downloadTask tvShow to pretend the parent already exists and the episode and season need to be created.
                if (query.MediaKey == tvShowDb.Key)
                {
                    var result = MapperSetup.CreateMapper().Map<DownloadTask>(tvShowDb);
                    result.Id = 999;
                    return Result.Ok(result);
                }

                return Result.Fail("");
            });

        // Act
        var result = await _sut.GenerateTvShowEpisodesDownloadTasksAsync(episodeIds);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var tvShowDownloadTask = result.Value.First();
        tvShowDownloadTask.Id.ShouldBe(999);

        mock.VerifyMediator(It.IsAny<GetPlexTvShowEpisodeByIdQuery>, Times.Once);
        mock.VerifyMediator(It.IsAny<GetDownloadTaskByMediaKeyQuery>, Times.Exactly(3));

        tvShowDownloadTask.Children.ShouldAllBe(x => x.Id == 0);
        tvShowDownloadTask.Children.SelectMany(x => x.Children).ToList().ShouldAllBe(x => x.Id == 0);
        tvShowDownloadTask.Children.ShouldAllBe(x => x.ParentId == tvShowDownloadTask.Id);
        ShouldDownloadTask.ShouldTvShow(tvShowDownloadTask, tvShowDb);
    }
}