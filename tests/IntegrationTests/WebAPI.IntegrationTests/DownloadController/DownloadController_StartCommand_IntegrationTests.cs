using System.Linq;
using System.Threading.Tasks;
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
    [Collection("Sequential")]
    public class DownloadController_StartCommand_IntegrationTests : BaseIntegrationTests
    {
        public DownloadController_StartCommand_IntegrationTests(ITestOutputHelper output) : base(output) { }

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

            await CreateContainer(config);
            var downloadTasks = await Container.PlexRipperDbContext.DownloadTasks.ToListAsync();
            downloadTasks.Count.ShouldBe(10);

            // Act
            var response = await Container.GetAsync(ApiRoutes.Download.GetStartCommand(downloadTasks.First().Id));
            var result = await response.Deserialize<ResultDTO>();
            await Task.Delay(5000);

            // Assert
            result.IsSuccess.ShouldBeTrue();

            // TODO Add better success checks here
        }
    }
}