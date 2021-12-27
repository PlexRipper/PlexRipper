using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Logging;
using PlexRipper.BaseTests;
using PlexRipper.BaseTests.Extensions;
using PlexRipper.Settings.Models;
using PlexRipper.WebAPI.Common;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.Common.FluentResult;
using PlexRipper.WebAPI.SignalR.Common;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace WebAPI.IntegrationTests.DownloadController
{
    public class DownloadController_GetDownloadTasks_IntegrationTests
    {
        public DownloadController_GetDownloadTasks_IntegrationTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public async Task ShouldHaveAllDownloadTasksNested_WhenTasksAreAvailable()
        {
            // Arrange
            var config = new UnitTestDataConfig
            {
                Seed = 4564,
                TvShowDownloadTasksCount = 5,
            };

            var container = new BaseContainer(config);

            // Act
            var response = await container.ApiClient.GetAsync(ApiRoutes.Download.GetDownloadTasks);
            var result = await response.Deserialize<ResultDTO<List<ServerDownloadProgressDTO>>>();

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
            result.IsSuccess.ShouldBeTrue();
            var plexServer = result.Value.First();
            plexServer.ShouldNotBeNull();
            plexServer.Downloads.Count.ShouldBe(5);
            plexServer.Downloads.ShouldAllBe(x => x.Children.Count == 5);
        }
    }
}