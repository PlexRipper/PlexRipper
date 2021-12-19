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
    public class DownloadQueue_ServerCompletedDownloading_UnitTests
    {
        public DownloadQueue_ServerCompletedDownloading_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public void ShouldHaveNoUpdates_WhenGivenAnEmptyList()
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

            List<int> serverCompleted = new();
            var plexServers = FakeData.GetPlexServer(config).Generate(1);
            foreach (var downloadTask in plexServers[0].PlexLibraries[0].DownloadTasks)
            {
                downloadTask.DownloadStatus = DownloadStatus.Completed;
            }

            // Act
            _sut.ServerCompletedDownloading.Subscribe(serverId => serverCompleted.Add(serverId));
            _sut.ExecuteDownloadQueue(plexServers);

            // Assert
            serverCompleted.Any().ShouldBeTrue();
        }
    }
}