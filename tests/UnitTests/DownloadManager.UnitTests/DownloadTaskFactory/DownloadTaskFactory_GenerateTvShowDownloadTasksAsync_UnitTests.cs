using Data.Contracts;
using PlexRipper.Application;
using PlexRipper.BaseTests.Asserts;
using PlexRipper.DownloadManager;

namespace DownloadManager.UnitTests;

public class DownloadTaskFactory_GenerateTvShowDownloadTasksAsync_UnitTests : BaseUnitTest<DownloadTaskFactory>
{
    public DownloadTaskFactory_GenerateTvShowDownloadTasksAsync_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldHaveFailedResult_WhenPlexTvShowsAreEmpty()
    {
        // Arrange
        await SetupDatabase();
        var tvShowIds = new List<int>();

        // Act
        var result = await _sut.GenerateTvShowDownloadTasksAsync(tvShowIds);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldGenerateValidTvShowDownloadTaskWithAllEpisodesDownloadTask_WhenNoDownloadTasksExist()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.TvShowCount = 5;
            config.TvShowSeasonCount = 2;
            config.TvShowEpisodeCount = 5;
            config.TvShowDownloadTasksCount = 1;
        });

        var tvShows = DbContext.PlexTvShows.IncludeEpisodes().IncludePlexServer().IncludePlexLibrary().ToList();

        mock.SetupMediator(It.IsAny<GetPlexTvShowByIdWithEpisodesQuery>)
            .ReturnsAsync((GetPlexTvShowByIdWithEpisodesQuery query, CancellationToken _) => Result.Ok(tvShows.Find(x => x.Id == query.PlexTvShowId)));

        var tvShowIds = new List<int> { 1 };

        // Act
        var result = await _sut.GenerateTvShowDownloadTasksAsync(tvShowIds);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(tvShowIds.Count);
        var tvShowDownloadTask = result.Value.First();

        ShouldDownloadTask.ShouldTvShow(tvShowDownloadTask, tvShows[0]);
    }
}