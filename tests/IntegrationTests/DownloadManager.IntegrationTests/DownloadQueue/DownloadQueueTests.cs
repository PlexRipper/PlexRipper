using System;
using System.Collections.Generic;
using System.Linq;
using FluentResultExtensions.lib;
using Logging;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using Shouldly;
using WireMock.Server;
using Xunit;
using Xunit.Abstractions;

namespace DownloadManager.IntegrationTests.DownloadQueue
{
    public class DownloadQueueTests
    {
        private BaseContainer Container { get; }

        private readonly WireMockServer _server;


        public DownloadQueueTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
            Container = new BaseContainer();

            _server = Container.MockServer.GetPlexMockServer();

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
        public void ExecuteDownloadQueue_ShouldHaveOneStartCommand_WhenNoCurrentTaskIsDownloading()
        {
            //Arrange
            var downloadQueue = Container.GetDownloadQueue;
            var serverList = Container.FakeData.GetPlexServer().Generate(1);
            serverList[0].PlexLibraries = Container.FakeData.GetPlexLibrary(serverList[0].Id, 1, PlexMediaType.Movie).Generate(1);
            serverList[0].PlexLibraries[0].DownloadTasks = Container.FakeData.GetMovieDownloadTask().Generate(5);

            List<int> startCommands = new();

            // Act
            downloadQueue.StartDownloadTask.Subscribe(command => startCommands.Add(command));
            downloadQueue.ExecuteDownloadQueue(serverList);

            // Assert
            startCommands.Count.ShouldBeEquivalentTo(1);
        }

      [Fact]
        public void ExecuteDownloadQueue_ShouldHaveANewTaskWithDownloadingStatus_WhenATaskIsSetToCompleted()
        {
            //Arrange
            var downloadQueue = Container.GetDownloadQueue;
            var serverList = Container.FakeData.GetPlexServer().Generate(1);
            serverList[0].PlexLibraries = Container.FakeData.GetPlexLibrary(serverList[0].Id, 1, PlexMediaType.Movie).Generate(1);
            serverList[0].PlexLibraries[0].DownloadTasks = Container.FakeData.GetMovieDownloadTask().Generate(5);
            serverList[0].PlexLibraries[0].DownloadTasks[0].DownloadStatus = DownloadStatus.Completed;

            List<int> startCommands = new();
            downloadQueue.StartDownloadTask.Subscribe(command => startCommands.Add(command));

            // Act
            downloadQueue.ExecuteDownloadQueue(serverList);

            // Assert
            startCommands.Count.ShouldBeEquivalentTo(1);
        }

        [Fact]
        public void ExecuteDownloadQueue_ShouldHaveNoNewTaskWithDownloadingStatus_WhenATaskIsAlreadyDownloading()
        {
            //Arrange
            var downloadQueue = Container.GetDownloadQueue;
            var serverList = Container.FakeData.GetPlexServer().Generate(1);
            serverList[0].PlexLibraries = Container.FakeData.GetPlexLibrary(serverList[0].Id, 1, PlexMediaType.Movie).Generate(1);
            serverList[0].PlexLibraries[0].DownloadTasks = Container.FakeData.GetMovieDownloadTask().Generate(5);
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