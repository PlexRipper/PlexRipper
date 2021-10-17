using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Bogus.Extensions;
using FluentResults;
using Logging;
using MediatR;
using Moq;
using PlexRipper.Application.Common;
using PlexRipper.Application.PlexTvShows;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using PlexRipper.WebAPI.Config;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace PlexRipper.Application.UnitTests.PlexDownloads
{
    public class PlexDownloadTaskFactory_GenerateTvShowDownloadTasksAsync_UnitTests
    {
        private readonly Mock<PlexDownloadTaskFactory> _sut;

        private readonly Mock<IMediator> _iMediator = new();

        private readonly Mock<IPlexAuthenticationService> _plexAuthenticationService = new();

        private readonly Mock<INotificationsService> _notificationsService = new();

        private readonly Mock<IFolderPathService> _folderPathService = new();

        private readonly Mock<IUserSettings> _userSettings = new();

        public PlexDownloadTaskFactory_GenerateTvShowDownloadTasksAsync_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);

            _sut = new Mock<PlexDownloadTaskFactory>(
                MockBehavior.Strict,
                _iMediator.Object,
                MapperSetup.CreateMapper(),
                _plexAuthenticationService.Object,
                _notificationsService.Object,
                _folderPathService.Object,
                _userSettings.Object);
        }

        [Fact]
        public async Task ShouldHaveFailedResult_WhenPlexTvShowsAreEmpty()
        {
            // Arrange
            _iMediator.Setup(x => x.Send(It.IsAny<GetPlexTvShowTreeByMediaIdsQuery>(), CancellationToken.None)).ReturnsAsync(Result.Fail(""));
            var tvShowIds = new List<int>();
            var tvShowSeasonIds = new List<int>();
            var tvShowEpisodeIds = new List<int>();

            // Act
            var result = await _sut.Object.GenerateDownloadTvShowTasksAsync(tvShowIds, tvShowSeasonIds, tvShowEpisodeIds);

            // Assert
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ShouldHaveValidDownloadTasks_WhenPlexTvShowsAreValid()
        {
            // Arrange
            var tvShows = FakeData.GetPlexTvShows().Generate(5);
            _iMediator.Setup(x => x.Send(It.IsAny<GetPlexTvShowTreeByMediaIdsQuery>(), CancellationToken.None)).ReturnsAsync(Result.Ok(tvShows));

            var tvShowIds = new List<int>();
            var tvShowSeasonIds = new List<int>();
            var tvShowEpisodeIds = new List<int>();

            // Act
            var result = await _sut.Object.GenerateDownloadTvShowTasksAsync(tvShowIds, tvShowSeasonIds, tvShowEpisodeIds);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Count.ShouldBe(5);

            for (var i = 0; i < tvShows.Count; i++)
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
                downloadTask.MediaId.ShouldBe(tvShow.Id);

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
                    seasonDownloadTask.MediaId.ShouldBe(tvShowSeason.Id);

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
                        episodeDownloadTask.MediaId.ShouldBe(tvShowEpisode.Id);

                        episodeDownloadTask.MediaType.ShouldBe(tvShowEpisode.Type);
                        episodeDownloadTask.DownloadTaskType.ShouldBe(DownloadTaskType.Episode);
                        episodeDownloadTask.DownloadStatus.ShouldBe(DownloadStatus.Initialized);
                        episodeDownloadTask.Created.ShouldBeGreaterThan(DateTime.MinValue);
                        episodeDownloadTask.Created.ShouldBeLessThan(DateTime.UtcNow);

                        var plexMediaDataParts = tvShowEpisode.EpisodeData.SelectMany(x => x.Parts).ToList();

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
                                episodeDataPartDownloadTask.MediaId.ShouldBe(tvShowEpisode.Id);

                                episodeDataPartDownloadTask.MediaType.ShouldBe(tvShowEpisode.Type);
                                episodeDataPartDownloadTask.DownloadTaskType.ShouldBe(tvShowEpisodeData.Parts.Count == 1
                                    ? DownloadTaskType.EpisodeData
                                    : DownloadTaskType.EpisodePart);
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