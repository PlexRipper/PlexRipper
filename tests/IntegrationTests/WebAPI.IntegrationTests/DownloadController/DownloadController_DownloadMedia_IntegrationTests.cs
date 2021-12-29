using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Logging;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.BaseTests;
using PlexRipper.BaseTests.Extensions;
using PlexRipper.Data.Common;
using PlexRipper.Domain;
using PlexRipper.Domain.DownloadManager;
using PlexRipper.WebAPI.Common;
using PlexRipper.WebAPI.Common.FluentResult;
using PlexRipper.WebAPI.SignalR.Common;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace WebAPI.IntegrationTests.DownloadController
{
    public class DownloadController_DownloadMedia_IntegrationTests
    {
        public DownloadController_DownloadMedia_IntegrationTests(ITestOutputHelper output)
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
                TvShowCount = 1,
                TvShowSeasonCount = 1,
                TvShowEpisodeCount = 5,
                MockServerConfig = new PlexMockServerConfig(50),
                DownloadSpeedLimit = 2000,
            };

            var container = await BaseContainer.Create(config);
            var plexServers = container.PlexRipperDbContext.PlexServers.ToList();
            foreach (var plexServer in plexServers)
            {
                container.GetUserSettings.SetDownloadSpeedLimit(new DownloadSpeedLimitModel
                {
                    PlexServerId = plexServer.Id,
                    MachineIdentifier = plexServer.MachineIdentifier,
                    DownloadSpeedLimit = config.DownloadSpeedLimit,
                });
            }

            // Setup sometimes needs a bit longer
            await Task.Delay(1000);

            var plexTvShow = await container.PlexRipperDbContext.PlexTvShows.IncludeEpisodes().FirstOrDefaultAsync(x => x.Id == 1);
            if (plexTvShow is null)
            {
                var dbContext = container.PlexRipperDbContext;
                plexTvShow.ShouldNotBeNull();
            }

            var request = new List<DownloadMediaDTO>
            {
                new()
                {
                    MediaIds = new List<int>
                    {
                        1,
                    },
                    Type = PlexMediaType.TvShow,
                },
            };

            // Act
            var response = await container.PostAsync(ApiRoutes.Download.PostDownloadMedia, request);
            var result = await response.Deserialize<ResultDTO<List<ServerDownloadProgressDTO>>>();
            await Task.Delay(60000);
            await container.GetDownloadTracker.DownloadProcessTask;

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
            result.IsSuccess.ShouldBeTrue();

            // var plexServer = result.Value.First();
            // plexServer.ShouldNotBeNull();
            // plexServer.Downloads.Count.ShouldBe(5);
            // plexServer.Downloads.ShouldAllBe(x => x.Children.Count == 5);
            //container.Dispose();
        }
    }
}