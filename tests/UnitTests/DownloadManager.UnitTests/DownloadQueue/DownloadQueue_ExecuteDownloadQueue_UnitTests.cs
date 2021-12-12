using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.Moq;
using Logging;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using PlexRipper.DownloadManager;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace DownloadManager.UnitTests
{
    public class DownloadQueue_ExecuteDownloadQueue_UnitTests
    {
        public DownloadQueue_ExecuteDownloadQueue_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public void ShouldHaveNoUpdates_WhenGivenAnEmptyList()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<DownloadQueue>();

            List<DownloadTask> updates = new();
            List<DownloadTask> startCommands = new();

            // Act
            _sut.UpdateDownloadTasks.Subscribe(update => updates = update);
            _sut.StartDownloadTask.Subscribe(command => startCommands.Add(command));
            _sut.ExecuteDownloadQueue(new List<PlexServer>());

            // Assert
            updates.Any().ShouldBeFalse();
            startCommands.Any().ShouldBeFalse();
        }

        [Fact]
        public void ShouldHaveNoStartCommands_WhenATaskIsAlreadyDownloading()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<DownloadQueue>();
            var config = new UnitTestDataConfig
            {
                IncludeLibraries = true,
                PlexLibraryCount = 1,
                LibraryType = PlexMediaType.Movie,
                IncludeDownloadTasks = true,
                DownloadTasksCount = 2,
            };

            List<DownloadTask> startCommands = new();
            var plexServers = FakeData.GetPlexServer(config).Generate(1);
            var startedDownloadTask = plexServers[0].PlexLibraries[0].DownloadTasks[0];
            startedDownloadTask.DownloadStatus = DownloadStatus.Downloading;

            // Act
            _sut.StartDownloadTask.Subscribe(command => startCommands.Add(command));
            _sut.ExecuteDownloadQueue(new List<PlexServer>());

            // Assert
            startCommands.Any().ShouldBeFalse();
        }

        [Fact]
        public void ShouldHaveNoDownloadTasksInitialized_WhenGivenDownloadTasksWithInitialized()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<DownloadQueue>();
            int updateIndex = 1;
            Dictionary<int, List<DownloadTask>> updates = new();
            var config = new UnitTestDataConfig
            {
                IncludeLibraries = true,
                PlexLibraryCount = 1,
                LibraryType = PlexMediaType.Movie,
                IncludeDownloadTasks = true,
                DownloadTasksCount = 2,
            };
            var plexServers = FakeData.GetPlexServer(config).Generate(1);

            // Act
            _sut.UpdateDownloadTasks.Subscribe(update => updates.Add(++updateIndex, update));
            _sut.ExecuteDownloadQueue(plexServers);

            // Assert

            var downloadTasks = updates[updateIndex];
            downloadTasks.Any().ShouldBeTrue();
            downloadTasks.All(x => x.DownloadStatus is not DownloadStatus.Initialized).ShouldBeTrue();
        }

        [Fact]
        public void ShouldHaveOneDownloadTaskStarted_WhenGivenMovieDownloadTasks()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<DownloadQueue>();
            List<DownloadTask> startCommands = new();
            var config = new UnitTestDataConfig
            {
                Seed = 5000,
                IncludeLibraries = true,
                PlexLibraryCount = 1,
                LibraryType = PlexMediaType.Movie,
                IncludeDownloadTasks = true,
                DownloadTasksCount = 2,
            };
            var plexServers = FakeData.GetPlexServer(config).Generate(1);

            // Act
            _sut.StartDownloadTask.Subscribe(command => startCommands.Add(command));
            _sut.ExecuteDownloadQueue(plexServers);

            // Assert
            var startedDownloadTask = plexServers[0].PlexLibraries[0].DownloadTasks[0].Children[0];
            startCommands.Count.ShouldBe(1);
            startCommands[0].Id.ShouldBe(startedDownloadTask.Id);
        }

        [Fact]
        public void ShouldHaveOneDownloadTaskDownloadingStatus_WhenGivenMovieDownloadTasks()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<DownloadQueue>();
            int updateIndex = 1;
            Dictionary<int, List<DownloadTask>> updates = new();
            var config = new UnitTestDataConfig
            {
                Seed = 25,
                IncludeLibraries = true,
                PlexLibraryCount = 1,
                LibraryType = PlexMediaType.Movie,
                IncludeDownloadTasks = true,
                DownloadTasksCount = 2,
            };
            var plexServers = FakeData.GetPlexServer(config).Generate(1);

            // Act
            _sut.UpdateDownloadTasks.Subscribe(update => updates.Add(++updateIndex, update));
            _sut.ExecuteDownloadQueue(plexServers);

            // Assert

            var downloadTasks = updates[updateIndex];
            downloadTasks.Any().ShouldBeTrue();
            downloadTasks[0].DownloadStatus.ShouldBe(DownloadStatus.Downloading);
            downloadTasks[0].Children[0].DownloadStatus.ShouldBe(DownloadStatus.Downloading);

            downloadTasks[1].DownloadStatus.ShouldBe(DownloadStatus.Queued);
            downloadTasks[1].Children[0].DownloadStatus.ShouldBe(DownloadStatus.Queued);
        }

        [Fact]
        public void ShouldHaveOneDownloadTaskDownloadingStatus_WhenGivenTvShowDownloadTasks()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<DownloadQueue>();
            int updateIndex = 1;
            Dictionary<int, List<DownloadTask>> updates = new();
            var config = new UnitTestDataConfig
            {
                Seed = 67,
                IncludeLibraries = true,
                PlexLibraryCount = 1,
                LibraryType = PlexMediaType.TvShow,
                IncludeDownloadTasks = true,
                DownloadTasksCount = 2,
            };
            var plexServers = FakeData.GetPlexServer(config).Generate(1);

            // Act
            _sut.UpdateDownloadTasks.Subscribe(update => updates.Add(++updateIndex, update));
            _sut.ExecuteDownloadQueue(plexServers);

            // Assert

            var downloadTasks = updates[updateIndex];
            downloadTasks.Any().ShouldBeTrue();
            downloadTasks[0].DownloadStatus.ShouldBe(DownloadStatus.Downloading);
            downloadTasks[0].Children[0].DownloadStatus.ShouldBe(DownloadStatus.Downloading);

            downloadTasks[1].DownloadStatus.ShouldBe(DownloadStatus.Queued);
            downloadTasks[1].Children.ShouldAllBe(x => x.DownloadStatus == DownloadStatus.Queued);
        }

        [Fact]
        public void ShouldHaveNextQueuedDownloadTask_WhenGivenAMovieDownloadTasksWithCompleted()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<DownloadQueue>();
            int updateIndex = 0;
            Dictionary<int, List<DownloadTask>> updates = new();
            var config = new UnitTestDataConfig
            {
                Seed = 67,
                IncludeLibraries = true,
                PlexLibraryCount = 1,
                LibraryType = PlexMediaType.Movie,
                IncludeDownloadTasks = true,
                DownloadTasksCount = 2,
            };
            var plexServers = FakeData.GetPlexServer(config).Generate(1);

            // Set first task to Completed
            var movieDownloadTask = plexServers[0].PlexLibraries[0].DownloadTasks[0];
            movieDownloadTask.DownloadStatus = DownloadStatus.Completed;
            movieDownloadTask.Children.ForEach(x => x.DownloadStatus = DownloadStatus.Completed);

            // Act
            _sut.UpdateDownloadTasks.Subscribe(update => updates.Add(++updateIndex, update));
            _sut.ExecuteDownloadQueue(plexServers);

            // Assert
            var downloadTasks = updates[updateIndex];
            downloadTasks.Any().ShouldBeTrue();
            downloadTasks[1].DownloadStatus.ShouldBe(DownloadStatus.Downloading);
            downloadTasks[1].Children[0].DownloadStatus.ShouldBe(DownloadStatus.Downloading);
        }

        [Fact]
        public void ShouldHaveNextQueuedDownloadTask_WhenGivenATvShowsDownloadTasksWithCompleted()
        {
            List<DownloadTask> SetToCompleted(List<DownloadTask> downloadTasks)
            {
                foreach (var downloadTask in downloadTasks)
                {
                    downloadTask.DownloadStatus = DownloadStatus.Completed;
                    if (downloadTask.Children.Any())
                    {
                        downloadTask.Children = SetToCompleted(downloadTask.Children);
                    }
                }

                return downloadTasks;
            }

            // Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<DownloadQueue>();
            int updateIndex = 0;
            Dictionary<int, List<DownloadTask>> updates = new();
            var config = new UnitTestDataConfig
            {
                Seed = 263,
                IncludeLibraries = true,
                PlexLibraryCount = 1,
                LibraryType = PlexMediaType.TvShow,
                IncludeDownloadTasks = true,
                DownloadTasksCount = 2,
            };
            var plexServers = FakeData.GetPlexServer(config).Generate(1);

            // Set first task to Completed
            var tvShowDownloadTask = plexServers[0].PlexLibraries[0].DownloadTasks[0];
            tvShowDownloadTask.DownloadStatus = DownloadStatus.Completed;
            tvShowDownloadTask.Children = SetToCompleted(tvShowDownloadTask.Children);

            // Act
            _sut.UpdateDownloadTasks.Subscribe(update => updates.Add(++updateIndex, update));
            _sut.ExecuteDownloadQueue(plexServers);

            // Assert
            var downloadTasks = updates[updateIndex];
            downloadTasks.Any().ShouldBeTrue();
            downloadTasks[1].DownloadStatus.ShouldBe(DownloadStatus.Downloading);
            downloadTasks[1].Children[0].DownloadStatus.ShouldBe(DownloadStatus.Downloading);
            downloadTasks[1].Children[0].Children[0].DownloadStatus.ShouldBe(DownloadStatus.Downloading);
            downloadTasks[1].Children[0].Children[1].DownloadStatus.ShouldBe(DownloadStatus.Queued);
            downloadTasks[1].Children[0].Children[0].Children[0].DownloadStatus.ShouldBe(DownloadStatus.Downloading);
        }

    }
}