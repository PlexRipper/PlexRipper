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
    public class GetAllDownloadTasksQueryHandler_UnitTests
    {
        public GetAllDownloadTasksQueryHandler_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public async Task ShouldReturnNoDownloadTasks_WhenNoDownloadTasksAreInDb()
        {
            // Arrange
            var config = new UnitTestDataConfig
            {
                Seed = 2867,
            };
            await using var context = MockDatabase.GetMemoryDbContext().Setup(config);
            var handle = new GetAllDownloadTasksQueryHandler(context);

            var request = new GetAllDownloadTasksQuery();

            // Act
            var result = await handle.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldBeEmpty();
        }

        [Fact]
        public async Task ShouldReturnMovieDownloadTasks_WhenMovieDownloadTasksAreInDB()
        {
            // Arrange
            var config = new UnitTestDataConfig
            {
                Seed = 21467,
                MovieDownloadTasksCount = 10,
            };
            await using var context = MockDatabase.GetMemoryDbContext().Setup(config);
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
        public async Task ShouldAllTvShowDownloadTasksWithAllIncludes_WhenTvShowDownloadTasksAreInDB()
        {
            // Arrange
            var config = new UnitTestDataConfig
            {
                Seed = 2767,
                TvShowDownloadTasksCount = 5,
                TvShowSeasonCountMax = 5,
                TvShowEpisodeCountMax = 5,
                LibraryType = PlexMediaType.TvShow,
            };
            await using var context = MockDatabase.GetMemoryDbContext().Setup(config);
            var handle = new GetAllDownloadTasksQueryHandler(context);
            var request = new GetAllDownloadTasksQuery();

            // Act
            var result = await handle.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            var downloadTasks = result.Value;
            downloadTasks.ShouldNotBeEmpty();

            void ValidateDownloadTasks(List<DownloadTask> shouldDownloadTasks)
            {
                downloadTasks.Count.ShouldBe(5);
                foreach (var downloadTask in shouldDownloadTasks)
                {
                    downloadTask.PlexServer.ShouldNotBeNull();
                    downloadTask.PlexLibrary.ShouldNotBeNull();
                    downloadTask.DownloadFolder.ShouldNotBeNull();
                    downloadTask.DestinationFolder.ShouldNotBeNull();
                    if (downloadTask.Children.Any())
                    {
                        ValidateDownloadTasks(downloadTask.Children);
                    }
                }
            }

            ValidateDownloadTasks(downloadTasks);
            downloadTasks.Flatten(x => x.Children).ToList().Count.ShouldBe(280);
        }
    }
}