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
using PlexRipper.Data;
using PlexRipper.Data.Common;
using PlexRipper.Domain;
using PlexRipper.DownloadManager;
using PlexRipper.WebAPI.Config;
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
            PlexRipperDbContext context = await MockDatabase.GetMemoryDbContext().Setup(config => { config.TvShowCount = 5; });
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
            tvShowDownloadTask.Id.ShouldBe(0);
            tvShowDownloadTask.Children.ShouldAllBe(x => x.Id == 0);
            tvShowDownloadTask.Children.SelectMany(x => x.Children).ToList().ShouldAllBe(x => x.Id == 0);
            ShouldDownloadTask.ShouldTvShow(tvShowDownloadTask, tvShowDb);
        }

        [Fact]
        public async Task ShouldGenerateValidEpisodeDownloadTask_WhenTvShowParentDownloadTaskAlreadyExist()
        {
            // Arrange
            var context = await MockDatabase.GetMemoryDbContext().Setup(config => { config.TvShowCount = 5; });

            var tvShows = await context.PlexTvShows.IncludeAll().ToListAsync();
            var tvShowDb = tvShows.Last();
            var episodeIds = new List<int> { tvShowDb.Seasons.First().Episodes.Last().Id };

            using var mock = AutoMock.GetStrict().AddMapper();
            mock.SetupMediator(It.IsAny<GetPlexTvShowByIdQuery>)
                .ReturnsAsync((GetPlexTvShowByIdQuery query, CancellationToken _) => Result.Ok(tvShows.Find(x => x.Id == query.Id)));

            mock.SetupMediator(It.IsAny<GetPlexTvShowEpisodeByIdQuery>)
                .ReturnsAsync((GetPlexTvShowEpisodeByIdQuery query, CancellationToken _) =>
                    Result.Ok(context.PlexTvShowEpisodes.IncludeAll().FirstOrDefault(x => x.Id == query.Id)));

            mock.SetupMediator(It.IsAny<GetDownloadTaskByMediaKeyQuery>).ReturnsAsync((GetDownloadTaskByMediaKeyQuery query, CancellationToken _) =>
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
            var _sut = mock.Create<DownloadTaskFactory>();
            var result = await _sut.GenerateTvShowEpisodesDownloadTasksAsync(episodeIds);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var tvShowDownloadTask = result.Value.First();
            tvShowDownloadTask.Id.ShouldBe(999);

            tvShowDownloadTask.Children.ShouldAllBe(x => x.Id == 0);
            tvShowDownloadTask.Children.SelectMany(x => x.Children).ToList().ShouldAllBe(x => x.Id == 0);
            tvShowDownloadTask.Children.ShouldAllBe(x => x.ParentId == tvShowDownloadTask.Id);
            ShouldDownloadTask.ShouldTvShow(tvShowDownloadTask, tvShowDb);
        }
    }
}