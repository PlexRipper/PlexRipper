using System.Linq;
using System.Threading.Tasks;
using Logging;
using Microsoft.EntityFrameworkCore;
using PlexRipper.BaseTests;
using PlexRipper.BaseTests.Extensions;
using PlexRipper.WebAPI.Common;
using PlexRipper.WebAPI.Common.FluentResult;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace WebAPI.IntegrationTests.DownloadController
{
    public class DownloadController_StartCommand_IntegrationTests
    {
        public DownloadController_StartCommand_IntegrationTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public async Task ShouldStartQueuedDownloadTaskOnStartCommand_WhenNoTasksAreDownloading()
        {
            // Arrange
            var config = new UnitTestDataConfig
            {
                Seed = 4564,
                MovieDownloadTasksCount = 5,
                MockServerConfig = new PlexMockServerConfig
                {
                    DownloadFileSizeInMb = 50,
                },
                MockDownloadSubscriptions = new MockDownloadSubscriptions(),
            };

            var container = await BaseContainer.Create(config);
            var downloadTasks = await container.PlexRipperDbContext.DownloadTasks.ToListAsync();
            downloadTasks.Count.ShouldBe(10);

            // Act
            var response = await container.GetAsync(ApiRoutes.Download.GetStartCommand(downloadTasks.First().Id));
            var result = await response.Deserialize<ResultDTO>();
            await Task.Delay(5000);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            // TODO Add better success checks here
        }
    }
}