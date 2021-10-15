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
using PlexRipper.Application.PlexMovies;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using PlexRipper.WebAPI.Config;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace PlexRipper.Application.UnitTests.PlexDownloads
{
    public class PlexDownloadTaskFactory_GenerateMovieDownloadTasksAsync_UnitTests
    {
        private readonly Mock<PlexDownloadTaskFactory> _sut;

        private readonly Mock<IMediator> _iMediator = new();

        private readonly Mock<IPlexAuthenticationService> _plexAuthenticationService = new();

        private readonly Mock<INotificationsService> _notificationsService = new();

        private readonly Mock<IFolderPathService> _folderPathService = new();

        private readonly Mock<IUserSettings> _userSettings = new();

        public PlexDownloadTaskFactory_GenerateMovieDownloadTasksAsync_UnitTests(ITestOutputHelper output)
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
        public async Task ShouldHaveFailedResult_WhenPlexMoviesAreEmpty()
        {
            // Arrange
            _iMediator.Setup(x => x.Send(It.IsAny<GetMultiplePlexMoviesByIdsQuery>(), CancellationToken.None)).ReturnsAsync(Result.Fail(""));
            var movies = new List<int>();

            // Act
            var result = await _sut.Object.GenerateMovieDownloadTasksAsync(movies);

            // Assert
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ShouldHaveValidDownloadTasks_WhenPlexMoviesAreValid()
        {
            // Arrange
            var movies = FakeData.GetPlexMovies(1, 1).Generate(5);
            _iMediator.Setup(x => x.Send(It.IsAny<GetMultiplePlexMoviesByIdsQuery>(), CancellationToken.None)).ReturnsAsync(Result.Ok(movies));

            // Act
            var result = await _sut.Object.GenerateMovieDownloadTasksAsync(movies.Select(x => x.Id).ToList());

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Count.ShouldBe(5);

            for (var i = 0; i < movies.Count; i++)
            {
                var plexMovie = movies[i];
                var downloadTask = result.Value[i];
                var fullTitle = $"{plexMovie.Title} ({plexMovie.Year})";

                downloadTask.Key.ShouldBe(plexMovie.Key);
                downloadTask.Title.ShouldBe(plexMovie.Title);
                downloadTask.FullTitle.ShouldBe(fullTitle);
                downloadTask.DataTotal.ShouldBe(plexMovie.MediaSize);
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
                downloadTask.Created.ShouldBeLessThan(DateTime.UtcNow);

                downloadTask.Children.ShouldNotBeEmpty();
                downloadTask.Children.Count.ShouldBe(plexMovie.MovieData.Count);

                for (var j = 0; j < plexMovie.MovieParts.Count; j++)
                {
                    var plexMoviePart = plexMovie.MovieParts[j];
                    var downloadTaskPart = downloadTask.Children[j];

                    downloadTaskPart.Key.ShouldBe(plexMovie.Key);
                    downloadTaskPart.Title.ShouldBe(plexMovie.Title);
                    downloadTaskPart.FullTitle.ShouldBe(plexMovie.FullTitle);
                    downloadTaskPart.DataTotal.ShouldBe(plexMoviePart.Size);
                    downloadTaskPart.Year.ShouldBe(plexMovie.Year);
                    downloadTaskPart.FileLocationUrl.ShouldNotBeEmpty();
                    downloadTaskPart.FileName.ShouldBe(Path.GetFileName(plexMoviePart.File));

                    downloadTaskPart.PlexLibrary.ShouldNotBeNull();
                    downloadTaskPart.PlexLibraryId.ShouldBeGreaterThan(0);
                    downloadTaskPart.PlexServer.ShouldNotBeNull();
                    downloadTaskPart.PlexServerId.ShouldBeGreaterThan(0);
                    downloadTaskPart.MediaId.ShouldBe(plexMovie.Id);

                    downloadTaskPart.MediaType.ShouldBe(plexMovie.Type);
                    // TODO Should check DownloadTaskType
                    downloadTaskPart.DownloadStatus.ShouldBe(DownloadStatus.Initialized);
                    downloadTaskPart.Created.ShouldBeGreaterThan(DateTime.MinValue);
                    downloadTaskPart.Created.ShouldBeLessThan(DateTime.UtcNow);

                    downloadTaskPart.Children.ShouldBeEmpty();
                    downloadTaskPart.DownloadWorkerTasks.ShouldNotBeEmpty();
                }
            }
        }
    }
}