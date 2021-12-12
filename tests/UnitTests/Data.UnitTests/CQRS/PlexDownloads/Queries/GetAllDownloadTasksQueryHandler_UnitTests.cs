using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Logging;
using PlexRipper.Application;
using PlexRipper.BaseTests;
using PlexRipper.Data;
using PlexRipper.Data.CQRS.PlexDownloads;
using PlexRipper.Domain;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Data.UnitTests
{
    public class GetAllDownloadTasksQueryHandler_UnitTests
    {
        public GetAllDownloadTasksQueryHandler_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public async Task ShouldReturnAllDownloadTasks_WhenNoIdsAreGiven()
        {
            // Arrange
            var config = new UnitTestDataConfig
            {
                Seed = 2867,
                DownloadTasksCount = 10,
                LibraryType = PlexMediaType.Movie,
            };
            await using var context = MockDatabase.GetMemoryDbContext().AddPlexServers().AddDownloadTasks(config);
            var handle = new GetAllDownloadTasksQueryHandler(context);

            var request = new GetAllDownloadTasksQuery();

            // Act
            var result = await handle.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var value = result.Value;
            value.ShouldNotBeEmpty();
            value.Count.ShouldBe(10);
        }

        [Fact]
        public async Task ShouldReturnMovieDownloadTasks_WhenMovieDownloadTasksAreInDB()
        {
            // Arrange
            var config = new UnitTestDataConfig
            {
                Seed = 21467,
                DownloadTasksCount = 10,
                LibraryType = PlexMediaType.Movie,
            };
            await using var context = MockDatabase.GetMemoryDbContext().AddPlexServers().AddDownloadTasks(config);
            var handle = new GetAllDownloadTasksQueryHandler(context);
            var request = new GetAllDownloadTasksQuery();

            // Act
            var result = await handle.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var downloadTasks = result.Value;
            downloadTasks.ShouldNotBeEmpty();
            downloadTasks.Count.ShouldBe(10);

            var flatList = downloadTasks.Flatten(x => x.Children).ToList();
            flatList.ShouldAllBe(x => x.PlexServer != null);
            flatList.ShouldAllBe(x => x.PlexLibrary != null);
            flatList.ShouldAllBe(x => x.DownloadFolder != null);
            flatList.ShouldAllBe(x => x.DestinationFolder != null);
        }

        [Fact]
        public async Task ShouldReturnTvShowDownloadTasks_WhenTvShowDownloadTasksAreInDB()
        {
            // Arrange
            var config = new UnitTestDataConfig
            {
                Seed = 21467,
                DownloadTasksCount = 10,
                TvShowSeasonCountMax = 1,
                TvShowEpisodeCountMax = 5,
                LibraryType = PlexMediaType.TvShow,
            };
            await using var context = MockDatabase.GetMemoryDbContext().AddPlexServers().AddDownloadTasks(config);
            var handle = new GetAllDownloadTasksQueryHandler(context);
            var request = new GetAllDownloadTasksQuery();

            // Act
            var result = await handle.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var downloadTasks = result.Value;
            downloadTasks.ShouldNotBeEmpty();
            downloadTasks.Count.ShouldBe(10);

            var flatList = downloadTasks.Flatten(x => x.Children).ToList();
            flatList.ShouldAllBe(x => x.PlexServer != null);
            flatList.ShouldAllBe(x => x.PlexLibrary != null);
            flatList.ShouldAllBe(x => x.DownloadFolder != null);
            flatList.ShouldAllBe(x => x.DestinationFolder != null);
        }
    }
}