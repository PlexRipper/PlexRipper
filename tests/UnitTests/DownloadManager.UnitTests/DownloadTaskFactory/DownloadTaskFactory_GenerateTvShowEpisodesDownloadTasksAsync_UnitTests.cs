using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using FluentResults;
using Logging;
using Microsoft.EntityFrameworkCore;
using Moq;
using PlexRipper.Application;
using PlexRipper.BaseTests;
using PlexRipper.BaseTests.Asserts;
using PlexRipper.BaseTests.Extensions;
using PlexRipper.Data.Common;
using PlexRipper.DownloadManager;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace DownloadManager.UnitTests
{
    public class DownloadTaskFactory_GenerateTvShowEpisodesDownloadTasksAsync_UnitTests
    {
        public DownloadTaskFactory_GenerateTvShowEpisodesDownloadTasksAsync_UnitTests(ITestOutputHelper output)
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
            var result = await _sut.GenerateTvShowEpisodesDownloadTasksAsync(tvShowIds);

            // Assert
            result.IsFailed.ShouldBeTrue();
        }
    
        [Fact]
        public async Task ShouldGenerateValidTvShowDownloadTaskWithEpisodeDownloadTask_WhenNoDownloadTasksExist()
        {
            // Arrange
            var config = new UnitTestDataConfig
            {
                TvShowCount = 5,
            };
            var context = MockDatabase.GetMemoryDbContext().Setup(config);
            var tvShows = await context.PlexTvShows.IncludeAll().ToListAsync();

            using var mock = AutoMock.GetStrict().AddMapper();
            mock.SetupMediator(It.IsAny<GetPlexTvShowByIdQuery>)
                .ReturnsAsync((GetPlexTvShowByIdQuery query, CancellationToken _) => Result.Ok(tvShows.Find(x => x.Id == query.Id)));

            mock.SetupMediator(It.IsAny<GetPlexTvShowEpisodeByIdQuery>)
                .ReturnsAsync((GetPlexTvShowEpisodeByIdQuery query, CancellationToken _) =>
                    Result.Ok(context.PlexTvShowEpisodes.IncludeAll().FirstOrDefault(x => x.Id == query.Id)));

            mock.SetupMediator(It.IsAny<GetDownloadTaskByMediaKeyQuery>).ReturnsAsync(Result.Fail(""));
            var _sut = mock.Create<DownloadTaskFactory>();

            var tvShowDb = tvShows.Last();
            var episodeIds = new List<int> { tvShowDb.Seasons.First().Episodes.Last().Id };

            // Act
            var result = await _sut.GenerateTvShowEpisodesDownloadTasksAsync(episodeIds);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var tvShowDownloadTask = result.Value.First();
            ShouldDownloadTask.ShouldTvShow(tvShowDownloadTask, tvShowDb);
        }
    }
}