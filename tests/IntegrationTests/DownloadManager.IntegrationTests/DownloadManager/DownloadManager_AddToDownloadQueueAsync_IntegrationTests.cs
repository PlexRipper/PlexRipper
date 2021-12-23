using System.Threading.Tasks;
using Logging;
using Microsoft.EntityFrameworkCore;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using PlexRipper.Domain.RxNet;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace DownloadManager.IntegrationTests.DownloadManager
{
    public class DownloadManager_AddToDownloadQueueAsync_IntegrationTests
    {
        public DownloadManager_AddToDownloadQueueAsync_IntegrationTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public async Task ShouldAddDownloadTasksAndStartOne_WhenGivenTwoValidMovieDownloadTasks()
        {
            // Arrange
            var config = new UnitTestDataConfig
            {
                MemoryDbName = MockDatabase.GetMemoryDatabaseName(),
                IncludeLibraries = true,
            };
            var container = new BaseContainer(config);

            var context = container.PlexRipperDbContext.AddPlexServers();
            var downloadTasks = FakeData.GetMovieDownloadTask(config).Generate(2);
            foreach (var downloadTask in downloadTasks)
            {
                downloadTask.PlexLibraryId = 1;
                downloadTask.PlexServerId = 1;
            }

            DownloadTask startDownloadTask = null;
            container.GetDownloadQueue.StartDownloadTask.SubscribeAsync(async x => startDownloadTask = x);

            // Act
            var result = await container.GetDownloadManager.AddToDownloadQueueAsync(downloadTasks);

            // Assert
            var downloadTasksDb = await context.DownloadTasks.ToListAsync();
            result.IsSuccess.ShouldBeTrue();
            downloadTasksDb.Count.ShouldBe(4);
            startDownloadTask.ShouldNotBeNull();
            startDownloadTask.Key.ShouldBe(downloadTasks[0].Children[0].Key);
        }
    }
}