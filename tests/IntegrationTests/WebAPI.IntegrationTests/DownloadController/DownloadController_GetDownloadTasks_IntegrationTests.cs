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
    [Collection("Sequential")]
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
                TvShowSeasonCount = 2,
                TvShowEpisodeCount = 2,
            };

            var container = await BaseContainer.Create(config);

            // Act
            var response = await container.ApiClient.GetAsync(ApiRoutes.Download.GetDownloadTasks);
            var result = await response.Deserialize<ResultDTO<List<ServerDownloadProgressDTO>>>();

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
            result.IsSuccess.ShouldBeTrue();
            var plexServer = result.Value.First();
            plexServer.ShouldNotBeNull();
            plexServer.Downloads.Count.ShouldBe(config.TvShowDownloadTasksCount);
            foreach (var downloadProgressDto in plexServer.Downloads)
            {
                downloadProgressDto.Children.Count.ShouldBe(config.TvShowSeasonDownloadTasksCount);
                foreach (var child in downloadProgressDto.Children)
                {
                    child.Children.Count.ShouldBe(config.TvShowEpisodeDownloadTasksCount);
                }
            }
        }
    }
}