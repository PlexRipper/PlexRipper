using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Logging;
using PlexRipper.Application;
using PlexRipper.BaseTests;
using PlexRipper.Data;
using PlexRipper.Domain;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Data.UnitTests
{
    public class GetAllRelatedDownloadTaskIdsQueryHandler_UnitTests
    {
        public GetAllRelatedDownloadTaskIdsQueryHandler_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public async Task ShouldReturnAllParentDownloadTasksIds_WhenChildDownloadTaskIdsAreGiven()
        {
            // Arrange
            var config = new UnitTestDataConfig
            {
                Seed = 2867,
                DownloadTasksCount = 10,
                LibraryType = PlexMediaType.Movie,
            };
            await using var context = await MockDatabase.GetMemoryDbContext().Setup(config);

            var childIds = new List<int> { 2, 4, 6, 8, 10 };
            var handle = new GetAllRelatedDownloadTaskIdsHandler(context);
            var request = new GetAllRelatedDownloadTaskIdsQuery(childIds);

            // Act
            var result = await handle.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var downloadTaskIds = result.Value;
            downloadTaskIds.ShouldNotBeEmpty();
            downloadTaskIds.Count.ShouldBe(10);
            downloadTaskIds.ShouldBeUnique();
            childIds.ShouldAllBe(x => downloadTaskIds.Contains(x));
        }

        [Fact]
        public async Task ShouldReturnAllRelatedDownloadTasksIds_WhenPlexTvShowDownloadTaskIdsGiven()
        {
            // Arrange
            var config = new UnitTestDataConfig
            {
                Seed = 2867,
                DownloadTasksCount = 10,
                LibraryType = PlexMediaType.TvShow,
            };
            await using var context = await MockDatabase.GetMemoryDbContext().Setup(config);

            var ids = context.DownloadTasks.Where(x => x.DownloadTaskType == DownloadTaskType.TvShow).Take(5).Select(x => x.Id).ToList();
            var handle = new GetAllRelatedDownloadTaskIdsHandler(context);
            var request = new GetAllRelatedDownloadTaskIdsQuery(ids);

            // Act
            var result = await handle.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var downloadTaskIds = result.Value;
            downloadTaskIds.ShouldNotBeEmpty();
            downloadTaskIds.Count.ShouldBe(530);
            downloadTaskIds.ShouldBeUnique();
            ids.ShouldAllBe(x => downloadTaskIds.Contains(x));
        }
    }
}