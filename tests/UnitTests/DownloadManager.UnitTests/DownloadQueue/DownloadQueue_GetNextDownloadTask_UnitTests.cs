using Data.Contracts;
using PlexRipper.DownloadManager;

namespace DownloadManager.UnitTests;

public class DownloadQueue_GetNextDownloadTask_UnitTests : BaseUnitTest<DownloadQueue>
{
    public DownloadQueue_GetNextDownloadTask_UnitTests(ITestOutputHelper output) : base(output) { }

    private List<DownloadTaskGeneric> TestDownloadTasks(int count)
    {
        var downloadTasks = new List<DownloadTaskGeneric>();
        var index = 1;
        for (var i = 0; i < count; i++)
        {
            var downloadTask_i = new DownloadTaskGeneric
            {
                Id = Guid.NewGuid(),
                DownloadStatus = DownloadStatus.Queued,
                Children = new List<DownloadTaskGeneric>(),
            };
            for (var j = 0; j < count; j++)
            {
                var downloadTask_j = new DownloadTaskGeneric
                {
                    Id = Guid.NewGuid(),
                    DownloadStatus = DownloadStatus.Queued,
                    Children = new List<DownloadTaskGeneric>(),
                };
                for (var k = 0; k < count; k++)
                {
                    var downloadTask_k = new DownloadTaskGeneric
                    {
                        Id = Guid.NewGuid(),
                        DownloadStatus = DownloadStatus.Queued,
                        Children = new List<DownloadTaskGeneric>(),
                    };
                    for (var l = 0; l < count; l++)
                    {
                        var downloadTask_l = new DownloadTaskGeneric
                        {
                            Id = Guid.NewGuid(),
                            DownloadStatus = DownloadStatus.Queued,
                            Children = new List<DownloadTaskGeneric>(),
                        };
                        downloadTask_k.Children.Add(downloadTask_l);
                    }

                    downloadTask_j.Children.Add(downloadTask_k);
                }

                downloadTask_i.Children.Add(downloadTask_j);
            }

            downloadTasks.Add(downloadTask_i);
        }

        return downloadTasks;
    }

    [Fact]
    public async Task ShouldHaveNextDownloadTask_WhenAllAreQueued()
    {
        // Arrange
        await SetupDatabase(config => { config.TvShowDownloadTasksCount = 5; });
        var downloadTasks = await DbContext.GetAllDownloadTasksAsync();

        // Act
        var nextDownloadTask = _sut.GetNextDownloadTask(downloadTasks);

        // Assert
        nextDownloadTask.IsSuccess.ShouldBeTrue();
        var nextDownloadTaskId = downloadTasks[0].Children[0].Children[0].Children[0].Id;
        nextDownloadTask.Value.Id.ShouldBe(nextDownloadTaskId);
    }

    [Fact]
    public async Task ShouldHaveNextDownloadTask_WhenADownloadTaskHasBeenCompleted()
    {
        // Arrange
        await SetupDatabase(config => { config.TvShowDownloadTasksCount = 5; });
        var downloadTasks = await DbContext.GetAllDownloadTasksAsync(asTracking: true);
        downloadTasks[0].SetDownloadStatus(DownloadStatus.Completed);
        await DbContext.SaveChangesAsync();

        // Act
        var nextDownloadTask = _sut.GetNextDownloadTask(downloadTasks);

        // Assert
        nextDownloadTask.IsSuccess.ShouldBeTrue();
        var nextDownloadTaskId = downloadTasks[1].Children[0].Children[0].Children[0].Id;
        nextDownloadTask.Value.Id.ShouldBe(nextDownloadTaskId);
    }

    [Fact]
    public async Task ShouldHaveNextQueuedDownloadTaskInDownloadingTask_WhenAParentDownloadTaskIsAlreadyDownloading()
    {
        // Arrange
        await SetupDatabase(config => { config.TvShowDownloadTasksCount = 5; });

        var downloadTasks = await DbContext.GetAllDownloadTasksAsync(asTracking: true);
        downloadTasks[0].SetDownloadStatus(DownloadStatus.Downloading);
        foreach (var child in downloadTasks[0].Children)
            child.SetDownloadStatus(DownloadStatus.Queued);
        await DbContext.SaveChangesAsync();

        // Act
        var nextDownloadTask = _sut.GetNextDownloadTask(downloadTasks);

        // Assert
        nextDownloadTask.IsSuccess.ShouldBeTrue();
        var nextDownloadTaskId = downloadTasks[0].Children[0].Children[0].Children[0].Id;
        nextDownloadTask.Value.Id.ShouldBe(nextDownloadTaskId);
    }

    [Fact]
    public async Task ShouldHaveNoDownloadTask_WhenADownloadTaskIsAlreadyDownloading()
    {
        // Arrange
        await SetupDatabase(config => { config.TvShowDownloadTasksCount = 5; });

        var downloadTasks = await DbContext.GetAllDownloadTasksAsync(asTracking: true);
        downloadTasks[0].SetDownloadStatus(DownloadStatus.Downloading);
        await DbContext.SaveChangesAsync();

        // Act
        var nextDownloadTask = _sut.GetNextDownloadTask(downloadTasks);

        // Assert
        nextDownloadTask.IsSuccess.ShouldBeFalse();
    }


    [Fact]
    public async Task ShouldHaveNextQueuedDownloadTask_WhenNestedInsideATvShowsDownloadTasksWithDownloadFinished()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.TvShowCount = 10;
            config.TvShowDownloadTasksCount = 2;
            config.TvShowSeasonDownloadTasksCount = 2;
            config.TvShowEpisodeDownloadTasksCount = 5;
        });

        var downloadTasks = await DbContext.GetAllDownloadTasksAsync(asTracking: true);
        downloadTasks[0].SetDownloadStatus(DownloadStatus.Completed);
        downloadTasks[1].SetDownloadStatus(DownloadStatus.DownloadFinished);
        downloadTasks[1].Children[0].SetDownloadStatus(DownloadStatus.Queued);
        await DbContext.SaveChangesAsync();

        // Act
        var nextDownloadTask = _sut.GetNextDownloadTask(downloadTasks);

        // Assert
        nextDownloadTask.IsSuccess.ShouldBeTrue();
        var nextDownloadTaskId = downloadTasks[1].Children[0].Children[0].Children[0].Id;
        nextDownloadTask.Value.Id.ShouldBe(nextDownloadTaskId);
    }
}