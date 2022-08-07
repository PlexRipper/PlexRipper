using PlexRipper.Data.Common;

namespace Data.UnitTests.Extensions;

public class PlexRipperDbContextExtensions_IncludeDownloadTasks_UnitTests
{
    public PlexRipperDbContextExtensions_IncludeDownloadTasks_UnitTests(ITestOutputHelper output)
    {
        Log.SetupTestLogging(output);
    }

    [Fact]
    public async Task ShouldHaveAllMovieDownloadTaskChildrenIncluded_WhenDbContainsNestedDownloadTasks()
    {
        // Arrange
        await using var context = await MockDatabase.GetMemoryDbContext().Setup(config => config.MovieDownloadTasksCount = 5);

        // Act
        var downloadTasks = context.DownloadTasks.IncludeDownloadTasks().Where(x => x.ParentId == null).ToList();

        // Assert
        downloadTasks.Count.ShouldBe(5);
        foreach (var downloadTask in downloadTasks)
        {
            downloadTask.Children.Count.ShouldBe(1);
            downloadTask.Children[0].ParentId.ShouldBe(downloadTask.Id);
        }
    }

    [Fact]
    public async Task ShouldHaveAllTvShowDownloadTaskChildrenIncluded_WhenDbContainsNestedDownloadTasks()
    {
        // Arrange
        await using var context = await MockDatabase.GetMemoryDbContext()
            .Setup(config =>
            {
                config.Seed = 3535;
                config.TvShowDownloadTasksCount = 5;
                config.TvShowSeasonDownloadTasksCount = 5;
                config.TvShowEpisodeDownloadTasksCount = 5;
            });

        // Act
        var downloadTasksDb = context.DownloadTasks.IncludeDownloadTasks().Where(x => x.ParentId == null).ToList();

        // Assert
        downloadTasksDb.Count.ShouldBe(5);
        downloadTasksDb.Flatten(x => x.Children).Count().ShouldBe(280);
    }

    [Fact]
    public async Task ShouldHaveAllNestedRelationshipsIncluded_WhenGivenTvShowDownloadTasks()
    {
        // Arrange
        await using var context = await MockDatabase.GetMemoryDbContext()
            .Setup(config =>
            {
                config.Seed = 3882;
                config.TvShowDownloadTasksCount = 5;
                config.TvShowSeasonDownloadTasksCount = 5;
                config.TvShowEpisodeDownloadTasksCount = 5;
            });

        // Act
        var downloadTasksDb = context.DownloadTasks.IncludeDownloadTasks().Where(x => x.ParentId == null).ToList();

        // Assert
        downloadTasksDb.Count.ShouldBe(5);
        foreach (var downloadTask in downloadTasksDb)
        {
            downloadTask.Children.Count.ShouldBe(5);
            downloadTask.Parent.ShouldBeNull();
            foreach (var downloadTask2 in downloadTask.Children)
            {
                downloadTask2.Children.Count.ShouldBe(5);
                downloadTask2.Parent.ShouldNotBeNull();
                foreach (var downloadTask3 in downloadTask2.Children)
                {
                    downloadTask3.Parent.ShouldNotBeNull();
                    downloadTask3.Children.Count.ShouldBe(1);
                }
            }
        }

        void CheckChildren(List<DownloadTask> downloadTasks)
        {
            foreach (var downloadTask in downloadTasks)
            {
                downloadTask.PlexServer.ShouldNotBeNull();
                downloadTask.PlexLibrary.ShouldNotBeNull();
                downloadTask.DownloadFolder.ShouldNotBeNull();
                downloadTask.DestinationFolder.ShouldNotBeNull();
                if (downloadTask.Children.Any())
                    CheckChildren(downloadTask.Children);
            }
        }

        CheckChildren(downloadTasksDb);
    }
}