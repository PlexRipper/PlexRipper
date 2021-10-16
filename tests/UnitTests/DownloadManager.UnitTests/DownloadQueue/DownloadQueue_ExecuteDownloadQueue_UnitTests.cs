using System;
using System.Collections.Generic;
using System.Linq;
using Logging;
using Moq;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using PlexRipper.DownloadManager;
using PlexRipper.WebAPI.Config;
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
            List<int> startCommands = new();
            var downloadQueue = new DownloadQueue();

            // Act
            downloadQueue.UpdateDownloadTask.Subscribe(update => updates.Add(update));
            downloadQueue.StartDownloadTask.Subscribe(command => startCommands.Add(command));
            downloadQueue.ExecuteDownloadQueue(new List<PlexServer>());

            // Assert
            updates.Any().ShouldBeFalse();
            startCommands.Any().ShouldBeFalse();
        }
    }
}