using Autofac.Extras.Moq;
using Moq;
using PlexRipper.Application;
using PlexRipper.BaseTests;
using PlexRipper.BaseTests.Asserts;
using PlexRipper.BaseTests.Extensions;
using PlexRipper.Data;
using PlexRipper.Data.Common;
using PlexRipper.DownloadManager;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace DownloadManager.UnitTests
{
    public class DownloadTaskFactory_GenerateTvShowDownloadTasksAsync_UnitTests
    {
        public DownloadTaskFactory_GenerateTvShowDownloadTasksAsync_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public async Task ShouldHaveFailedResult_WhenPlexTvShowsAreEmpty()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<DownloadTaskFactory>();
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
            await using PlexRipperDbContext context = await MockDatabase.GetMemoryDbContext().Setup(config => { config.TvShowCount = 1; });
            var tvShows = context.PlexTvShows.IncludeEpisodes().IncludePlexServer().IncludePlexLibrary().ToList();

            using var mock = AutoMock.GetStrict().AddMapper();
            mock.SetupMediator(It.IsAny<GetPlexTvShowByIdWithEpisodesQuery>)
                .ReturnsAsync((GetPlexTvShowByIdWithEpisodesQuery query, CancellationToken _) => Result.Ok(tvShows.Find(x => x.Id == query.Id)));
            mock.SetupMediator(It.IsAny<GetDownloadTaskByMediaKeyQuery>)
                .ReturnsAsync(Result.Fail(""));
            var _sut = mock.Create<DownloadTaskFactory>();

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
}