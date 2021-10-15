using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Environment;
using FluentResults;
using Logging;
using MediatR;
using Moq;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.WebApi;
using PlexRipper.Application.FolderPaths;
using PlexRipper.Application.PlexLibraries;
using PlexRipper.Application.PlexTvShows;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using PlexRipper.WebAPI.Config;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace PlexRipper.Application.UnitTests.PlexDownloads
{
    public class PlexDownloadTaskFactory_GenerateAsync_UnitTests
    {
        private readonly Mock<PlexDownloadTaskFactory> _sut;

        private readonly Mock<IMediator> _iMediator = new();

        private readonly Mock<IPlexAuthenticationService> _plexAuthenticationService = new();

        private readonly Mock<INotificationsService> _notificationsService = new();

        private readonly Mock<IFolderPathService> _folderPathService = new();

        private readonly Mock<IUserSettings> _userSettings = new();

        public PlexDownloadTaskFactory_GenerateAsync_UnitTests(ITestOutputHelper output)
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
        public async Task ShouldHaveFailedResult_WhenFolderPathsAreInvalid()
        {
            // Arrange
            _iMediator.Setup(x => x.Send(It.IsAny<GetPlexTvShowTreeByMediaIdsQuery>(), CancellationToken.None)).ReturnsAsync(Result.Fail(""));
            _folderPathService.Setup(x => x.CheckIfFolderPathsAreValid(PlexMediaType.None)).ReturnsAsync(Result.Fail(""));
            var tvShowIds = new List<int>();
            var tvShowSeasonIds = new List<int>();
            var tvShowEpisodeIds = new List<int>();

            var downloadMedias = new List<DownloadMediaDTO>()
            {
                new()
                {
                    Type = PlexMediaType.TvShow,
                    MediaIds = tvShowIds,
                },
                new()
                {
                    Type = PlexMediaType.Season,
                    MediaIds = tvShowSeasonIds,
                },
                new()
                {
                    Type = PlexMediaType.Episode,
                    MediaIds = tvShowEpisodeIds,
                },
            };

            // Act
            var result = await _sut.Object.GenerateAsync(downloadMedias);

            // Assert
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ShouldHaveValidDownloadTasks_WhenPlexTvShowsAreValid()
        {
            // Arrange
            var tvShows = FakeData.GetPlexTvShows(1, 1).Generate(5);
            var folderPaths = FakeData.GetFolderPaths().Generate(10);
            _folderPathService.Setup(x => x.CheckIfFolderPathsAreValid(PlexMediaType.None)).ReturnsAsync(Result.Ok());
            _folderPathService.Setup(x => x.GetDownloadFolderAsync()).ReturnsAsync(Result.Ok(new FolderPath()
            {
                Id = 1,
                FolderType = FolderType.DownloadFolder,
                DirectoryPath = Path.Combine(PathSystem.RootDirectory, "downloads"),
                PlexLibraries = new List<PlexLibrary>(),
                DisplayName = "Download Folder",
                MediaType = PlexMediaType.None,
            }));
            _folderPathService.Setup(x => x.GetDefaultDestinationFolderDictionary()).ReturnsAsync(Result.Ok(
                new Dictionary<PlexMediaType, FolderPath>()
                {
                    { PlexMediaType.Movie, folderPaths.FirstOrDefault(x => x.Id == 2) },
                    { PlexMediaType.TvShow, folderPaths.FirstOrDefault(x => x.Id == 3) },
                    { PlexMediaType.Season, folderPaths.FirstOrDefault(x => x.Id == 3) },
                    { PlexMediaType.Episode, folderPaths.FirstOrDefault(x => x.Id == 3) },
                    { PlexMediaType.Music, folderPaths.FirstOrDefault(x => x.Id == 4) },
                    { PlexMediaType.Album, folderPaths.FirstOrDefault(x => x.Id == 4) },
                    { PlexMediaType.Song, folderPaths.FirstOrDefault(x => x.Id == 4) },
                    { PlexMediaType.Photos, folderPaths.FirstOrDefault(x => x.Id == 5) },
                    { PlexMediaType.OtherVideos, folderPaths.FirstOrDefault(x => x.Id == 6) },
                    { PlexMediaType.Games, folderPaths.FirstOrDefault(x => x.Id == 7) },
                    { PlexMediaType.None, folderPaths.FirstOrDefault(x => x.Id == 1) },
                    { PlexMediaType.Unknown, folderPaths.FirstOrDefault(x => x.Id == 1) },
                }));
            _iMediator.Setup(x => x.Send(It.IsAny<GetPlexTvShowTreeByMediaIdsQuery>(), CancellationToken.None)).ReturnsAsync(Result.Ok(tvShows));
            _iMediator.Setup(x => x.Send(It.IsAny<GetAllPlexLibrariesQuery>(), CancellationToken.None)).ReturnsAsync(Result.Ok(new List<PlexLibrary>()
            {
                new()
                {
                    Id = 1,
                    PlexServer = new()
                    {
                        Id = 1,
                    },
                },
            }));
            _iMediator.Setup(x => x.Send(It.IsAny<GetAllFolderPathsQuery>(), CancellationToken.None)).ReturnsAsync(Result.Ok(folderPaths));
            _userSettings.Setup(x => x.DownloadSegments).Returns(4);

            var tvShowIds = tvShows.Select(x => x.Id).Take(5).ToList();
            var tvShowSeasonIds = new List<int>();
            var tvShowEpisodeIds = new List<int>();

            var downloadMedias = new List<DownloadMediaDTO>()
            {
                new()
                {
                    Type = PlexMediaType.TvShow,
                    MediaIds = tvShowIds,
                },
                new()
                {
                    Type = PlexMediaType.Season,
                    MediaIds = tvShowSeasonIds,
                },
                new()
                {
                    Type = PlexMediaType.Episode,
                    MediaIds = tvShowEpisodeIds,
                },
            };

            // Act
            var result = await _sut.Object.GenerateAsync(downloadMedias);

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

                downloadTask.DownloadDirectory.ShouldNotBeEmpty();
                downloadTask.DestinationDirectory.ShouldNotBeEmpty();

                downloadTask.PlexLibrary.ShouldNotBeNull();
                downloadTask.PlexLibraryId.ShouldBe(tvShow.PlexLibraryId);
                downloadTask.PlexServer.ShouldNotBeNull();
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

                    seasonDownloadTask.DownloadDirectory.ShouldNotBeEmpty();
                    seasonDownloadTask.DestinationDirectory.ShouldNotBeEmpty();

                    seasonDownloadTask.PlexLibrary.ShouldNotBeNull();
                    seasonDownloadTask.PlexLibraryId.ShouldBe(seasonDownloadTask.PlexLibraryId);
                    seasonDownloadTask.PlexServer.ShouldNotBeNull();
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

                        episodeDownloadTask.DownloadDirectory.ShouldNotBeEmpty();
                        episodeDownloadTask.DestinationDirectory.ShouldNotBeEmpty();

                        episodeDownloadTask.PlexLibrary.ShouldNotBeNull();
                        episodeDownloadTask.PlexLibraryId.ShouldBe(tvShowEpisode.PlexLibraryId);
                        episodeDownloadTask.PlexServer.ShouldNotBeNull();
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

                                episodeDataPartDownloadTask.FileName.ShouldNotBeEmpty();
                                episodeDataPartDownloadTask.FileName.ShouldNotContain(@"/");
                                episodeDataPartDownloadTask.FileName.ShouldNotContain(@"\");
                                episodeDataPartDownloadTask.FileLocationUrl.ShouldNotBeEmpty();
                                episodeDataPartDownloadTask.DownloadDirectory.ShouldNotBeEmpty();
                                episodeDataPartDownloadTask.DestinationDirectory.ShouldNotBeEmpty();

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