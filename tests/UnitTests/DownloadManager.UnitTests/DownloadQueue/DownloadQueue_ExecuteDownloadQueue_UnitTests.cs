using System;
using System.Collections.Generic;
using System.Linq;
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
            List<DownloadTask> updates = new();
            List<DownloadTask> startCommands = new();
            var downloadQueue = new DownloadQueue();

            // Act
            downloadQueue.UpdateDownloadTasks.Subscribe(update => updates = update);
            downloadQueue.StartDownloadTask.Subscribe(command => startCommands.Add(command));
            downloadQueue.ExecuteDownloadQueue(new List<PlexServer>());

            // Assert
            updates.Any().ShouldBeFalse();
            startCommands.Any().ShouldBeFalse();
        }

        [Fact]
        public void ShouldHaveNoStartCommands_WhenATaskIsAlreadyDownloading()
        {
            // Arrange
            var config = new FakeDataConfig
            {
                IncludeLibraries = true,
                LibraryCount = 1,
                LibraryType = PlexMediaType.Movie,
                IncludeDownloadTasks = true,
                DownloadTasksCount = 2,
            };

            List<DownloadTask> startCommands = new();
            var plexServers = FakeData.GetPlexServer(config).Generate(1);
            var downloadQueue = new DownloadQueue();
            var startedDownloadTask = plexServers[0].PlexLibraries[0].DownloadTasks[0];
            startedDownloadTask.DownloadStatus = DownloadStatus.Downloading;

            // Act
            downloadQueue.StartDownloadTask.Subscribe(command => startCommands.Add(command));
            downloadQueue.ExecuteDownloadQueue(new List<PlexServer>());

            // Assert
            startCommands.Any().ShouldBeFalse();
        }

        [Fact]
        public void ShouldHaveNoDownloadTasksInitialized_WhenGivenDownloadTasksWithInitialized()
        {
            // Arrange
            int updateIndex = 1;
            Dictionary<int, List<DownloadTask>> updates = new();
            var config = new FakeDataConfig
            {
                IncludeLibraries = true,
                LibraryCount = 1,
                LibraryType = PlexMediaType.Movie,
                IncludeDownloadTasks = true,
                DownloadTasksCount = 2,
            };
            var downloadQueue = new DownloadQueue();
            var plexServers = FakeData.GetPlexServer(config).Generate(1);

            // Act
            downloadQueue.UpdateDownloadTasks.Subscribe(update => updates.Add(++updateIndex, update));
            downloadQueue.ExecuteDownloadQueue(plexServers);

            // Assert

            var downloadTasks = updates[updateIndex];
            downloadTasks.Any().ShouldBeTrue();
            downloadTasks.All(x => x.DownloadStatus is not DownloadStatus.Initialized).ShouldBeTrue();
        }

        [Fact]
        public void ShouldHaveOneDownloadTaskStarted_WhenGivenMovieDownloadTasks()
        {
            // Arrange
            List<DownloadTask> startCommands = new();
            var config = new FakeDataConfig
            {
                Seed = 5000,
                IncludeLibraries = true,
                LibraryCount = 1,
                LibraryType = PlexMediaType.Movie,
                IncludeDownloadTasks = true,
                DownloadTasksCount = 2,
            };
            var downloadQueue = new DownloadQueue();
            var plexServers = FakeData.GetPlexServer(config).Generate(1);

            // Act
            downloadQueue.StartDownloadTask.Subscribe(command => startCommands.Add(command));
            downloadQueue.ExecuteDownloadQueue(plexServers);

            // Assert
            var startedDownloadTask = plexServers[0].PlexLibraries[0].DownloadTasks[0].Children[0];
            startCommands.Count.ShouldBe(1);
            startCommands[0].Id.ShouldBe(startedDownloadTask.Id);
        }

        [Fact]
        public void ShouldHaveOneDownloadTaskDownloadingStatus_WhenGivenMovieDownloadTasks()
        {
            // Arrange
            int updateIndex = 1;
            Dictionary<int, List<DownloadTask>> updates = new();
            var config = new FakeDataConfig
            {
                Seed = 25,
                IncludeLibraries = true,
                LibraryCount = 1,
                LibraryType = PlexMediaType.Movie,
                IncludeDownloadTasks = true,
                DownloadTasksCount = 2,
            };
            var downloadQueue = new DownloadQueue();
            var plexServers = FakeData.GetPlexServer(config).Generate(1);

            // Act
            downloadQueue.UpdateDownloadTasks.Subscribe(update => updates.Add(++updateIndex, update));
            downloadQueue.ExecuteDownloadQueue(plexServers);

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
            int updateIndex = 1;
            Dictionary<int, List<DownloadTask>> updates = new();
            var config = new FakeDataConfig
            {
                Seed = 67,
                IncludeLibraries = true,
                LibraryCount = 1,
                LibraryType = PlexMediaType.TvShow,
                IncludeDownloadTasks = true,
                DownloadTasksCount = 2,
            };
            var downloadQueue = new DownloadQueue();
            var plexServers = FakeData.GetPlexServer(config).Generate(1);

            // Act
            downloadQueue.UpdateDownloadTasks.Subscribe(update => updates.Add(++updateIndex, update));
            downloadQueue.ExecuteDownloadQueue(plexServers);

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
            int updateIndex = 0;
            Dictionary<int, List<DownloadTask>> updates = new();
            var config = new FakeDataConfig
            {
                Seed = 67,
                IncludeLibraries = true,
                LibraryCount = 1,
                LibraryType = PlexMediaType.Movie,
                IncludeDownloadTasks = true,
                DownloadTasksCount = 2,
            };
            var downloadQueue = new DownloadQueue();
            var plexServers = FakeData.GetPlexServer(config).Generate(1);

            // Set first task to Completed
            var movieDownloadTask = plexServers[0].PlexLibraries[0].DownloadTasks[0];
            movieDownloadTask.DownloadStatus = DownloadStatus.Completed;
            movieDownloadTask.Children.ForEach(x => x.DownloadStatus = DownloadStatus.Completed);

            // Act
            downloadQueue.UpdateDownloadTasks.Subscribe(update => updates.Add(++updateIndex, update));
            downloadQueue.ExecuteDownloadQueue(plexServers);

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
            int updateIndex = 0;
            Dictionary<int, List<DownloadTask>> updates = new();
            var config = new FakeDataConfig
            {
                Seed = 263,
                IncludeLibraries = true,
                LibraryCount = 1,
                LibraryType = PlexMediaType.TvShow,
                IncludeDownloadTasks = true,
                DownloadTasksCount = 2,
            };
            var downloadQueue = new DownloadQueue();
            var plexServers = FakeData.GetPlexServer(config).Generate(1);

            // Set first task to Completed
            var tvShowDownloadTask = plexServers[0].PlexLibraries[0].DownloadTasks[0];
            tvShowDownloadTask.DownloadStatus = DownloadStatus.Completed;
            tvShowDownloadTask.Children = SetToCompleted(tvShowDownloadTask.Children);

            // Act
            downloadQueue.UpdateDownloadTasks.Subscribe(update => updates.Add(++updateIndex, update));
            downloadQueue.ExecuteDownloadQueue(plexServers);

            // Assert
            var downloadTasks = updates[updateIndex];
            downloadTasks.Any().ShouldBeTrue();
            downloadTasks[1].DownloadStatus.ShouldBe(DownloadStatus.Downloading);
            downloadTasks[1].Children[0].DownloadStatus.ShouldBe(DownloadStatus.Downloading);
            downloadTasks[1].Children[0].Children[0].DownloadStatus.ShouldBe(DownloadStatus.Downloading);
            downloadTasks[1].Children[0].Children[1].DownloadStatus.ShouldBe(DownloadStatus.Queued);
            downloadTasks[1].Children[0].Children[0].Children[0].DownloadStatus.ShouldBe(DownloadStatus.Downloading);
            downloadTasks[1].Children[0].Children[0].Children[1].DownloadStatus.ShouldBe(DownloadStatus.Queued);
        }
    }
}