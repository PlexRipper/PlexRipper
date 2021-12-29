using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Logging;
using Microsoft.EntityFrameworkCore;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace PlexRipper.Application.IntegrationTests
{
    public class PlexDownloadService_DownloadMediaAsync_IntegrationTests
    {
        public PlexDownloadService_DownloadMediaAsync_IntegrationTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public async Task ShouldDownloadMediaFiles_WhenEverythingIsValid()
        {
            // Arrange

            var config = new UnitTestDataConfig
            {
                Seed = 9999,
                MovieCount = 5,
            };
            var testContainer = await BaseContainer.Create(config);
            var moviesDb = await testContainer.PlexRipperDbContext.PlexMovies.ToListAsync();
            var downloadMediaDTO = new List<DownloadMediaDTO>
            {
                new()
                {
                    Type = PlexMediaType.Movie,
                    MediaIds = moviesDb.Select(x => x.Id).ToList(),
                },
            };

            // Act
            var syncResult = await testContainer.GetPlexDownloadService.DownloadMediaAsync(downloadMediaDTO);
            await Task.Delay(50000);
            await testContainer.GetDownloadTracker.DownloadProcessTask;

            // Assert
            syncResult.IsSuccess.ShouldBeTrue();
        }
    }
}