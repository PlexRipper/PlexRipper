using PlexRipper.Data.Common;

namespace Data.UnitTests.Extensions;

public class PlexRipperDbContextExtensions_IncludeDownloadTasks_UnitTests : BaseUnitTest
{
    public PlexRipperDbContextExtensions_IncludeDownloadTasks_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldHaveAllMovieDownloadTaskChildrenIncluded_WhenDbContainsNestedDownloadTasks()
    {
        // Arrange
        Seed = 334734;
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.MovieCount = 10;
            config.MovieDownloadTasksCount = 5;
        });

        // Act
        var downloadTasks = DbContext.DownloadTasks.IncludeDownloadTasks().Where(x => x.ParentId == null).ToList();

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
        Seed = 334734;
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.TvShowCount = 10;
            config.TvShowSeasonCount = 10;
            config.TvShowEpisodeCount = 10;
            config.TvShowDownloadTasksCount = 5;
            config.TvShowSeasonDownloadTasksCount = 5;
            config.TvShowEpisodeDownloadTasksCount = 5;
        });

        // Act
        var downloadTasksDb = DbContext.DownloadTasks.IncludeDownloadTasks().Where(x => x.ParentId == null).ToList();

        // Assert
        downloadTasksDb.Count.ShouldBe(5);
        downloadTasksDb.Flatten(x => x.Children).Count().ShouldBe(280);
    }

    [Fact]
    public async Task ShouldHaveAllNestedRelationshipsIncluded_WhenGivenTvShowDownloadTasks()
    {
        // Arrange
        Seed = 3882;
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.TvShowCount = 10;
            config.TvShowSeasonCount = 10;
            config.TvShowEpisodeCount = 10;
            config.TvShowDownloadTasksCount = 5;
            config.TvShowSeasonDownloadTasksCount = 5;
            config.TvShowEpisodeDownloadTasksCount = 5;
        });

        // Act
        var downloadTasksDb = DbContext.DownloadTasks.IncludeDownloadTasks().Where(x => x.ParentId == null).ToList();

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