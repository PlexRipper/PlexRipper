using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using FluentResults;
using Logging;
using Moq;
using PlexRipper.Application.PlexTvShows;
using PlexRipper.BaseTests;
using PlexRipper.BaseTests.Extensions;
using PlexRipper.Domain;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace PlexRipper.Application.UnitTests.PlexDownloads
{
    public class PlexDownloadTaskFactory_GenerateTvShowDownloadTasksAsync_UnitTests
    {

        public PlexDownloadTaskFactory_GenerateTvShowDownloadTasksAsync_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public async Task ShouldHaveFailedResult_WhenPlexTvShowsAreEmpty()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<PlexDownloadTaskFactory>();
            var tvShowIds = new List<int>();

            // Act
            var result = await _sut.GenerateTvShowDownloadTasksAsync(tvShowIds);

            // Assert
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ShouldHaveValidDownloadTasks_WhenGivenAValidPlexTvShowId()
        {
            // Arrange
            var tvShows = FakeData.GetPlexTvShows().Generate(5);
            using var mock = AutoMock.GetStrict().AddMapper();
            mock.SetupMediator(It.IsAny<GetPlexTvShowByIdQuery>).ReturnsAsync(Result.Ok(tvShows.Find(x => x.Id == 1)));
            mock.SetupMediator(It.IsAny<GetDownloadTaskByMediaKeyQuery>).ReturnsAsync(Result.Fail(""));
            var _sut = mock.Create<PlexDownloadTaskFactory>();

            var tvShowIds = new List<int> { 1 };

            // Act
            var result = await _sut.GenerateTvShowDownloadTasksAsync(tvShowIds);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Count.ShouldBe(tvShowIds.Count);

            for (var i = 0; i < tvShowIds.Count; i++)
            {
                var tvShow = tvShows[i];
                var downloadTask = result.Value[i];

                downloadTask.Key.ShouldBe(tvShow.Key);
                downloadTask.Title.ShouldBe(tvShow.Title);
                downloadTask.FullTitle.ShouldBe(tvShow.FullTitle);
                downloadTask.DataTotal.ShouldBe(tvShow.MediaSize);
                downloadTask.Year.ShouldBe(tvShow.Year);

                downloadTask.PlexLibraryId.ShouldBe(tvShow.PlexLibraryId);
                downloadTask.PlexServerId.ShouldBe(tvShow.PlexServerId);

                downloadTask.MediaType.ShouldBe(tvShow.Type);
                downloadTask.DownloadTaskType.ShouldBe(DownloadTaskType.TvShow);
                downloadTask.DownloadStatus.ShouldBe(DownloadStatus.Initialized);
                downloadTask.Created.ShouldBeGreaterThan(DateTime.MinValue);
                downloadTask.Created.ShouldBeLessThan(DateTime.UtcNow);

                downloadTask.Children.ShouldNotBeEmpty();
                downloadTask.Children.Count.ShouldBe(tvShow.Seasons.Count);

                for (var j = 0; j < tvShow.Seasons.Count; j++)
                {
                    var tvShowSeason = tvShow.Seasons[j];
                    var seasonDownloadTask = downloadTask.Children[j];

                    seasonDownloadTask.Key.ShouldBe(tvShowSeason.Key);
                    seasonDownloadTask.Title.ShouldBe(tvShowSeason.Title);
                    seasonDownloadTask.FullTitle.ShouldBe(tvShowSeason.FullTitle);
                    seasonDownloadTask.DataTotal.ShouldBe(tvShowSeason.MediaSize);
                    seasonDownloadTask.Year.ShouldBe(tvShowSeason.Year);

                    seasonDownloadTask.PlexLibraryId.ShouldBe(seasonDownloadTask.PlexLibraryId);
                    seasonDownloadTask.PlexServerId.ShouldBe(seasonDownloadTask.PlexServerId);

                    seasonDownloadTask.MediaType.ShouldBe(tvShowSeason.Type);

                    seasonDownloadTask.DownloadTaskType.ShouldBe(DownloadTaskType.Season);
                    seasonDownloadTask.DownloadStatus.ShouldBe(DownloadStatus.Initialized);
                    seasonDownloadTask.Created.ShouldBeGreaterThan(DateTime.MinValue);
                    seasonDownloadTask.Created.ShouldBeLessThan(DateTime.UtcNow);

                    seasonDownloadTask.Children.ShouldNotBeEmpty();
                    seasonDownloadTask.Children.Count.ShouldBe(tvShowSeason.Episodes.Count);

                    for (int k = 0; k < tvShowSeason.Episodes.Count; k++)
                    {
                        var tvShowEpisode = tvShowSeason.Episodes[k];
                        var episodeDownloadTask = seasonDownloadTask.Children[k];

                        episodeDownloadTask.Key.ShouldBe(tvShowEpisode.Key);
                        episodeDownloadTask.Title.ShouldBe(tvShowEpisode.Title);
                        episodeDownloadTask.FullTitle.ShouldBe(tvShowEpisode.FullTitle);
                        episodeDownloadTask.DataTotal.ShouldBe(tvShowEpisode.MediaSize);
                        episodeDownloadTask.Year.ShouldBe(tvShowEpisode.Year);

                        episodeDownloadTask.PlexLibraryId.ShouldBe(tvShowEpisode.PlexLibraryId);
                        episodeDownloadTask.PlexServerId.ShouldBe(tvShowEpisode.PlexServerId);

                        episodeDownloadTask.MediaType.ShouldBe(tvShowEpisode.Type);
                        episodeDownloadTask.DownloadTaskType.ShouldBe(DownloadTaskType.Episode);
                        episodeDownloadTask.DownloadStatus.ShouldBe(DownloadStatus.Initialized);
                        episodeDownloadTask.Created.ShouldBeGreaterThan(DateTime.MinValue);
                        episodeDownloadTask.Created.ShouldBeLessThan(DateTime.UtcNow);

                        var plexMediaDataParts = tvShowEpisode.EpisodeData.First().Parts;
                        if (plexMediaDataParts.Count > 1)
                        {
                            episodeDownloadTask.Children.ShouldNotBeEmpty();
                            episodeDownloadTask.Children.Count.ShouldBe(plexMediaDataParts.Count);

                            for (int m = 0; m < tvShowEpisode.EpisodeData.Count; m++)
                            {
                                var tvShowEpisodeData = tvShowEpisode.EpisodeData[m];

                                for (int l = 0; l < tvShowEpisodeData.Parts.Count; l++)
                                {
                                    var tvShowEpisodeDataPart = tvShowEpisodeData.Parts[l];
                                    var episodeDataPartDownloadTask = episodeDownloadTask.Children[m + l];

                                    episodeDataPartDownloadTask.Key.ShouldBe(tvShowEpisode.Key);
                                    episodeDataPartDownloadTask.Title.ShouldBe(tvShowEpisode.Title);
                                    episodeDataPartDownloadTask.FullTitle.ShouldBe(tvShowEpisode.FullTitle);
                                    episodeDataPartDownloadTask.DataTotal.ShouldBe(tvShowEpisodeDataPart.Size);
                                    episodeDataPartDownloadTask.Year.ShouldBe(tvShowEpisode.Year);

                                    episodeDataPartDownloadTask.PlexLibraryId.ShouldBe(tvShowEpisode.PlexLibraryId);
                                    episodeDataPartDownloadTask.PlexServerId.ShouldBe(tvShowEpisode.PlexServerId);

                                    episodeDataPartDownloadTask.MediaType.ShouldBe(tvShowEpisode.Type);
                                    episodeDataPartDownloadTask.DownloadTaskType.ShouldBe(DownloadTaskType.EpisodePart);
                                    episodeDataPartDownloadTask.DownloadStatus.ShouldBe(DownloadStatus.Initialized);
                                    episodeDataPartDownloadTask.Created.ShouldBeGreaterThan(DateTime.MinValue);
                                    episodeDataPartDownloadTask.Created.ShouldBeLessThan(DateTime.UtcNow);
                                    episodeDataPartDownloadTask.DownloadWorkerTasks.ShouldNotBeEmpty();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}