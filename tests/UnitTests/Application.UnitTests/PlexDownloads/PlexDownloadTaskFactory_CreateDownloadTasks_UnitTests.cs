using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bogus.Extensions;
using FluentResults;
using Logging;
using MediatR;
using Moq;
using PlexRipper.Application.Common;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace PlexRipper.Application.UnitTests.PlexDownloads
{
    public class PlexDownloadTaskFactory_CreateDownloadTasks_UnitTests
    {
        private readonly Mock<PlexDownloadTaskFactory> _sut;

        private readonly Mock<IMediator> _iMediator = new();

        private readonly Mock<IPlexAuthenticationService> _plexAuthenticationService = new();

        private readonly Mock<INotificationsService> _notificationsService = new();

        private readonly Mock<IFolderPathService> _folderPathService = new();

        private readonly Mock<IUserSettings> _userSettings = new();

        public PlexDownloadTaskFactory_CreateDownloadTasks_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);

            _sut = new Mock<PlexDownloadTaskFactory>(
                MockBehavior.Strict,
                _iMediator.Object,
                _plexAuthenticationService.Object,
                _notificationsService.Object,
                _folderPathService.Object, _userSettings.Object);
        }

        [Fact]
        public void CreateDownloadTasks_ShouldHaveFailedResult_WhenPlexMoviesAreEmpty()
        {
            // Arrange
            var movies = new List<PlexMovie>();

            // Act
            var result = _sut.Object.CreateDownloadTasks(movies);

            // Assert
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public void CreateDownloadTasks_ShouldHaveValidDownloadTasks_WhenPlexMoviesAreValid()
        {
            // Arrange
            var movies = FakeData.GetPlexMovies(1, 1).Generate(5);

            // Act
            var result = _sut.Object.CreateDownloadTasks(movies);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Count.ShouldBe(5);

            for (var i = 0; i < movies.Count; i++)
            {
                var plexMovie = movies[i];
                var downloadTask = result.Value[i];

                downloadTask.Key.ShouldBe(plexMovie.Key);
                downloadTask.Title.ShouldBe(plexMovie.Title);
                downloadTask.DataTotal.ShouldBe(plexMovie.MovieData.SelectMany(x => x.Parts).Sum(x => x.Size));
                downloadTask.Year.ShouldBe(plexMovie.Year);

                downloadTask.PlexLibrary.ShouldNotBeNull();
                downloadTask.PlexLibraryId.ShouldBeGreaterThan(0);
                downloadTask.PlexServer.ShouldNotBeNull();
                downloadTask.PlexServerId.ShouldBeGreaterThan(0);
                downloadTask.MediaId.ShouldBe(plexMovie.Id);

                downloadTask.MediaType.ShouldBe(plexMovie.Type);
                downloadTask.DownloadTaskType.ShouldBe(DownloadTaskType.Movie);
                downloadTask.DownloadStatus.ShouldBe(DownloadStatus.Initialized);
                downloadTask.Created.ShouldBeGreaterThan(DateTime.MinValue);
                downloadTask.Created.ShouldBeLessThan(DateTime.MaxValue);

                downloadTask.Children.ShouldNotBeEmpty();
                downloadTask.Children.Count.ShouldBe(plexMovie.MovieData.Count);
                for (var j = 0; j < plexMovie.MovieData.Count; j++)
                {
                    var downloadTaskData = downloadTask.Children[j];
                    var plexMovieData = plexMovie.MovieData[j];

                    downloadTaskData.Key.ShouldBe(plexMovie.Key);
                    downloadTaskData.Title.ShouldBe(plexMovie.Title);
                    downloadTaskData.Year.ShouldBe(plexMovie.Year);
                    downloadTask.DataTotal.ShouldBe(plexMovieData.Parts.Sum(x => x.Size));

                    downloadTaskData.PlexLibrary.ShouldNotBeNull();
                    downloadTaskData.PlexLibraryId.ShouldBeGreaterThan(0);
                    downloadTaskData.PlexServer.ShouldNotBeNull();
                    downloadTaskData.PlexServerId.ShouldBeGreaterThan(0);
                    downloadTaskData.MediaId.ShouldBe(plexMovie.Id);

                    downloadTaskData.MediaType.ShouldBe(plexMovie.Type);
                    downloadTaskData.DownloadTaskType.ShouldBe(DownloadTaskType.MovieData);
                    downloadTaskData.DownloadStatus.ShouldBe(DownloadStatus.Initialized);
                    downloadTaskData.Created.ShouldBeGreaterThan(DateTime.MinValue);
                    downloadTaskData.Created.ShouldBeLessThan(DateTime.MaxValue);

                    downloadTask.Children.ShouldNotBeEmpty();
                    downloadTask.Children.Count.ShouldBe(plexMovieData.Parts.Count);

                    for (var k = 0; k < plexMovieData.Parts.Count; k++)
                    {
                        var downloadTaskPart = downloadTaskData.Children[k];
                        var plexMoviePart = plexMovieData.Parts[k];

                        downloadTaskPart.Key.ShouldBe(plexMovie.Key);
                        downloadTaskPart.Title.ShouldBe(plexMovie.Title);
                        downloadTaskPart.DataTotal.ShouldBe(plexMoviePart.Size);
                        downloadTaskPart.Year.ShouldBe(plexMovie.Year);
                        downloadTaskPart.FileName.ShouldBe(Path.GetFileName(plexMoviePart.File));

                        downloadTaskPart.PlexLibrary.ShouldNotBeNull();
                        downloadTaskPart.PlexLibraryId.ShouldBeGreaterThan(0);
                        downloadTaskPart.PlexServer.ShouldNotBeNull();
                        downloadTaskPart.PlexServerId.ShouldBeGreaterThan(0);
                        downloadTaskPart.MediaId.ShouldBe(plexMovie.Id);

                        downloadTaskPart.MediaType.ShouldBe(plexMovie.Type);
                        downloadTaskPart.DownloadTaskType.ShouldBe(DownloadTaskType.MoviePart);
                        downloadTaskPart.DownloadStatus.ShouldBe(DownloadStatus.Initialized);
                        downloadTaskPart.Created.ShouldBeGreaterThan(DateTime.MinValue);
                        downloadTaskPart.Created.ShouldBeLessThan(DateTime.MaxValue);

                        downloadTaskPart.Children.ShouldBeEmpty();
                    }
                }
            }
        }
    }
}