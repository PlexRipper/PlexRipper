using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRipper.Application.PlexDownloads;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using Shouldly;
using WireMock.Server;
using Xunit;
using Xunit.Abstractions;

namespace DownloadManager.Tests.UnitTests
{
    public class DownloadManagerTests
    {
        private BaseContainer Container { get; }

        private readonly WireMockServer _server;

        public DownloadManagerTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.SetupLogging(output);
            Container = new BaseContainer();

            _server = MockServer.GetPlexMockServer();

            Log.Debug($"Server running at: {_server.Urls[0]}");
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

        [Fact]
        public async Task AddToDownloadQueueAsync_ShouldAddOnlyValidDownloadTasks_WhenAMixOfValidDownloadTasksAreGiven()
        {
            //Arrange
            var downloadManager = Container.GetDownloadManager;
            var downloadTasks = FakeData.MovieDownloadTasks().Generate(2);
            downloadTasks.AddRange(new List<DownloadTask>
            {
                new(),
                new(),
            });

            // Act
            var result = await downloadManager.AddToDownloadQueueAsync(downloadTasks);
            var dbTasks = await Container.Mediator.Send(new GetAllDownloadTasksQuery());

            // Assert
            result.IsSuccess.ShouldBeTrue();
            dbTasks.Value.Count.ShouldBeEquivalentTo(2);
        }
    }
}