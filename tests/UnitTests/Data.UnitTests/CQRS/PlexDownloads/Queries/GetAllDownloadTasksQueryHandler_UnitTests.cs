using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Logging;
using PlexRipper.Application;
using PlexRipper.BaseTests;
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
            var config = new FakeDataConfig
            {
                Seed = 2867,
                DownloadTasksCount = 10,
                LibraryType = PlexMediaType.Movie,
            };
            await using var context = MockDatabase.GetMemoryDbContext().AddDownloadTasks(config);
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
        public async Task ShouldReturnDownloadTasks_WhenMovieIdsAreGiven()
        {
            // Arrange
            var config = new FakeDataConfig
            {
                Seed = 28467,
                DownloadTasksCount = 10,
                LibraryType = PlexMediaType.Movie,
            };
            await using var context = MockDatabase.GetMemoryDbContext().AddDownloadTasks(config);
            var downloadTaskIds = new List<int> { 2, 3, 4, 5 };
            var handle = new GetAllDownloadTasksQueryHandler(context);
            var request = new GetAllDownloadTasksQuery(downloadTaskIds);

            // Act
            var result = await handle.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var value = result.Value;
            value.ShouldNotBeEmpty();
            value.Count.ShouldBe(3);
        }

        [Fact]
        public async Task ShouldReturnDownloadTasks_WhenTvShowIdsAreGiven()
        {
            // Arrange
            var config = new FakeDataConfig
            {
                Seed = 21467,
                DownloadTasksCount = 10,
                TvShowSeasonCountMax = 1,
                TvShowEpisodeCountMax = 5,
                LibraryType = PlexMediaType.TvShow,
            };
            await using var context = MockDatabase.GetMemoryDbContext().AddDownloadTasks(config);
            var downloadTaskIds = new List<int> { 1, 2, 3, 73, 117, 119 };
            var handle = new GetAllDownloadTasksQueryHandler(context);
            var request = new GetAllDownloadTasksQuery(downloadTaskIds);

            // Act
            var result = await handle.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var value = result.Value;
            value.ShouldNotBeEmpty();
            value.Count.ShouldBe(2);
            value[0].Id.ShouldBe(1);
        }
    }
}