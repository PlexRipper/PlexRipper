using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResultExtensions.lib;
using Logging;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using Shouldly;
using WireMock.Server;
using Xunit;
using Xunit.Abstractions;

namespace DownloadManager.IntegrationTests.DownloadManager
{
    public class DownloadManagerTests
    {
        private BaseContainer Container { get; }

        public DownloadManagerTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
            Container = new BaseContainer();

            WireMockServer server = Container.MockServer.GetPlexMockServer();

            Log.Debug($"Server running at: {server.Urls[0]}");
        }

        [Fact]
        public async Task AddToDownloadQueueAsync_ShouldReturnFailedResult_WhenEmptyListIsGiven()
        {
            //Arrange
            var downloadManager = Container.GetDownloadManager;

            // Act
            var result = await downloadManager.AddToDownloadQueueAsync(new List<DownloadTask>());

            // Assert
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task AddToDownloadQueueAsync_ShouldReturnFailedResult_WhenInvalidDownloadTasksAreGiven()
        {
            //Arrange
            var downloadManager = Container.GetDownloadManager;

            // Act
            var result = await downloadManager.AddToDownloadQueueAsync(new List<DownloadTask>
            {
                new(),
                new(),
            });

            // Assert
            result.IsFailed.ShouldBeTrue();
        }

    }
}