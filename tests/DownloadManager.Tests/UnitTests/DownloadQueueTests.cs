using System;
using System.Collections.Generic;
using System.Linq;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using Shouldly;
using WireMock.Server;
using Xunit;
using Xunit.Abstractions;

namespace DownloadManager.Tests.UnitTests
{
    public class DownloadQueueTests
    {
        private BaseContainer Container { get; }

        private readonly WireMockServer _server;

        public DownloadQueueTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.SetupLogging(output);
            Container = new BaseContainer();

            _server = MockServer.GetPlexMockServer();

            Log.Debug($"Server running at: {_server.Urls[0]}");
        }

        [Fact]
        public void ExecuteDownloadQueue_ShouldHaveNoUpdates_WhenGivenAnEmptyList()
        {
            //Arrange
            var downloadQueue = Container.GetDownloadQueue;
            List<DownloadTask> updates = new();
            List<int> startCommands = new();

            // Act
            downloadQueue.UpdateDownloadTask.Subscribe(update => updates.Add(update));
            downloadQueue.StartDownloadTask.Subscribe(command => startCommands.Add(command));

            downloadQueue.ExecuteDownloadQueue(new List<PlexServer>());

            // Assert
            updates.Any().ShouldBeFalse();
            startCommands.Any().ShouldBeFalse();
        }

        [Fact]
        public void ExecuteDownloadQueue_ShouldHaveSixUpdates_WhenGivenFiveInitializedDownloadTasks()
        {
            //Arrange
            var downloadQueue = Container.GetDownloadQueue;
            var serverList = FakeData.GetPlexServer().Generate(1);
            serverList[0].PlexLibraries = FakeData.GetPlexLibrary(serverList[0].Id, 1, PlexMediaType.Movie).Generate(1);
            serverList[0].PlexLibraries[0].DownloadTasks = FakeData.GetMovieDownloadTask().Generate(5);

            List<DownloadTask> downloadTaskUpdates = new();

            // Act
            downloadQueue.UpdateDownloadTask.Subscribe(update => downloadTaskUpdates.Add(update));
            downloadQueue.ExecuteDownloadQueue(serverList);

            // Assert
            downloadTaskUpdates.Count.ShouldBeEquivalentTo(6);
        }

        [Fact]
        public void ExecuteDownloadQueue_ShouldHaveOneStartCommand_WhenNoCurrentTaskIsDownloading()
        {
            //Arrange
            var downloadQueue = Container.GetDownloadQueue;
            var serverList = FakeData.GetPlexServer().Generate(1);
            serverList[0].PlexLibraries = FakeData.GetPlexLibrary(serverList[0].Id, 1, PlexMediaType.Movie).Generate(1);
            serverList[0].PlexLibraries[0].DownloadTasks = FakeData.GetMovieDownloadTask().Generate(5);

            List<int> startCommands = new();

            // Act
            downloadQueue.StartDownloadTask.Subscribe(command => startCommands.Add(command));
            downloadQueue.ExecuteDownloadQueue(serverList);

            // Assert
            startCommands.Count.ShouldBeEquivalentTo(1);
        }

        [Fact]
        public void ExecuteDownloadQueue_ShouldHaveATaskWithDownloadingStatus_WhenATaskIsSetToDownloading()
        {
            //Arrange
            var downloadQueue = Container.GetDownloadQueue;
            var serverList = FakeData.GetPlexServer().Generate(1);
            serverList[0].PlexLibraries = FakeData.GetPlexLibrary(serverList[0].Id, 1, PlexMediaType.Movie).Generate(1);
            serverList[0].PlexLibraries[0].DownloadTasks = FakeData.GetMovieDownloadTask().Generate(5);

            List<int> startCommands = new();
            List<DownloadTask> downloadTasksUpdates = new();

            // Act
            downloadQueue.StartDownloadTask.Subscribe(command => startCommands.Add(command));
            downloadQueue.UpdateDownloadTask.Subscribe(downloadTasksUpdate => downloadTasksUpdates.Add(downloadTasksUpdate));
            downloadQueue.ExecuteDownloadQueue(serverList);

            // Assert
            startCommands.Count.ShouldBeEquivalentTo(1);
            downloadTasksUpdates.Last().DownloadStatus.ShouldBe(DownloadStatus.Downloading);
        }

        [Fact]
        public void ExecuteDownloadQueue_ShouldHaveANewTaskWithDownloadingStatus_WhenATaskIsSetToCompleted()
        {
            //Arrange
            var downloadQueue = Container.GetDownloadQueue;
            var serverList = FakeData.GetPlexServer().Generate(1);
            serverList[0].PlexLibraries = FakeData.GetPlexLibrary(serverList[0].Id, 1, PlexMediaType.Movie).Generate(1);
            serverList[0].PlexLibraries[0].DownloadTasks = FakeData.GetMovieDownloadTask().Generate(5);
            serverList[0].PlexLibraries[0].DownloadTasks[0].DownloadStatus = DownloadStatus.Completed;

            List<int> startCommands = new();
            List<DownloadTask> downloadTaskUpdates = new();
            downloadQueue.StartDownloadTask.Subscribe(command => startCommands.Add(command));
            downloadQueue.UpdateDownloadTask.Subscribe(downloadTasksUpdate =>
            {
                downloadTaskUpdates.Add(downloadTasksUpdate);
                var i = serverList[0].PlexLibraries[0].DownloadTasks.FindIndex(x => x.Id == downloadTasksUpdate.Id);
                serverList[0].PlexLibraries[0].DownloadTasks[i] = downloadTasksUpdate;
            });

            // Act
            downloadQueue.ExecuteDownloadQueue(serverList);

            // Assert
            startCommands.Count.ShouldBeEquivalentTo(1);
            serverList[0].PlexLibraries[0].DownloadTasks.Count(x => x.DownloadStatus == DownloadStatus.Downloading).ShouldBeEquivalentTo(1);
        }

        [Fact]
        public void ExecuteDownloadQueue_ShouldHaveNoNewTaskWithDownloadingStatus_WhenATaskIsAlreadyDownloading()
        {
            //Arrange
            var downloadQueue = Container.GetDownloadQueue;
            var serverList = FakeData.GetPlexServer().Generate(1);
            serverList[0].PlexLibraries = FakeData.GetPlexLibrary(serverList[0].Id, 1, PlexMediaType.Movie).Generate(1);
            serverList[0].PlexLibraries[0].DownloadTasks = FakeData.GetMovieDownloadTask().Generate(5);
            serverList[0].PlexLibraries[0].DownloadTasks[0].DownloadStatus = DownloadStatus.Downloading;

            List<int> startCommands = new();
            List<DownloadTask> updates = new();
            downloadQueue.StartDownloadTask.Subscribe(command => startCommands.Add(command));
            downloadQueue.UpdateDownloadTask.Subscribe(update =>
            {
                updates.Add(update);
                var i = serverList[0].PlexLibraries[0].DownloadTasks.FindIndex(x => x.Id == update.Id);
                serverList[0].PlexLibraries[0].DownloadTasks[i] = update;
            });

            // Act
            downloadQueue.ExecuteDownloadQueue(serverList);

            // Assert
            serverList[0].PlexLibraries[0].DownloadTasks.Count(x => x.DownloadStatus == DownloadStatus.Downloading).ShouldBeEquivalentTo(1);
            startCommands.Count.ShouldBeEquivalentTo(0);
        }
    }
}