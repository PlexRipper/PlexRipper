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
        public void ExecuteDownloadQueue_ShouldHaveOneStartCommand_WhenNoCurrentTaskIsDownloading()
        {
            //Arrange
            var downloadQueue = Container.GetDownloadQueue;
            var config = new FakeDataConfig()
            {
                IncludeLibraries = true,
                LibraryType = PlexMediaType.Movie,
                IncludeDownloadTasks = true,
            };
            var serverList = FakeData.GetPlexServer(config).Generate(1);

            List<DownloadTask> startCommands = new();

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
            var config = new FakeDataConfig()
            {
                IncludeLibraries = true,
                LibraryType = PlexMediaType.Movie,
                IncludeDownloadTasks = true,
            };
            var serverList = FakeData.GetPlexServer(config).Generate(1);

            serverList[0].PlexLibraries[0].DownloadTasks[0].DownloadStatus = DownloadStatus.Completed;

            List<DownloadTask> startCommands = new();
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
            var config = new FakeDataConfig()
            {
                IncludeLibraries = true,
                LibraryType = PlexMediaType.Movie,
                IncludeDownloadTasks = true,
            };
            var serverList = FakeData.GetPlexServer(config).Generate(1);
            serverList[0].PlexLibraries[0].DownloadTasks[0].DownloadStatus = DownloadStatus.Downloading;

            List<DownloadTask> startCommands = new();
            List<DownloadTask> updates = new();
            downloadQueue.StartDownloadTask.Subscribe(command => startCommands.Add(command));
            downloadQueue.UpdateDownloadTasks.Subscribe(update =>
            {
                // TODO Either delete or refactor
                updates.AddRange(update);
               // var i = serverList[0].PlexLibraries[0].DownloadTasks.FindIndex(x => x.Id == update.Id);
               // serverList[0].PlexLibraries[0].DownloadTasks[i] = update;
            });

            // Act
            downloadQueue.ExecuteDownloadQueue(serverList);

            // Assert
            serverList[0].PlexLibraries[0].DownloadTasks.Count(x => x.DownloadStatus == DownloadStatus.Downloading).ShouldBeEquivalentTo(1);
            startCommands.Count.ShouldBeEquivalentTo(0);
        }
    }
}