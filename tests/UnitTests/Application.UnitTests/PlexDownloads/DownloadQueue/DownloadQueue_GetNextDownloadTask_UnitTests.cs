using Data.Contracts;

namespace PlexRipper.Application.UnitTests;

public class DownloadQueue_GetNextDownloadTask_UnitTests : BaseUnitTest<DownloadQueue>
{
    public DownloadQueue_GetNextDownloadTask_UnitTests(ITestOutputHelper output) : base(output) { }

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
    public async Task ShouldHaveNoNextDownloadTask_WhenMergingAndDownloadFinished()
    {
        // Arrange
        await SetupDatabase(config => { config.MovieDownloadTasksCount = 5; });

        var downloadTasks = await DbContext.GetAllDownloadTasksAsync(asTracking: true);
        downloadTasks[0].SetDownloadStatus(DownloadStatus.Merging);
        downloadTasks[1].SetDownloadStatus(DownloadStatus.DownloadFinished);
        downloadTasks[2].SetDownloadStatus(DownloadStatus.DownloadFinished);
        downloadTasks[3].SetDownloadStatus(DownloadStatus.DownloadFinished);
        downloadTasks[4].SetDownloadStatus(DownloadStatus.DownloadFinished);
        await DbContext.SaveChangesAsync();

        // Act
        var nextDownloadTask = _sut.GetNextDownloadTask(downloadTasks);

        // Assert
        nextDownloadTask.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldHaveLastQueuedDownloadTask_WhenMergingQueuedAndDownloadFinished()
    {
        // Arrange
        await SetupDatabase(config => { config.MovieDownloadTasksCount = 5; });

        var downloadTasks = await DbContext.GetAllDownloadTasksAsync(asTracking: true);
        downloadTasks[0].SetDownloadStatus(DownloadStatus.Merging);
        downloadTasks[1].SetDownloadStatus(DownloadStatus.DownloadFinished);
        downloadTasks[2].SetDownloadStatus(DownloadStatus.DownloadFinished);
        downloadTasks[3].SetDownloadStatus(DownloadStatus.DownloadFinished);
        downloadTasks[4].SetDownloadStatus(DownloadStatus.Queued);
        await DbContext.SaveChangesAsync();

        // Act
        var nextDownloadTask = _sut.GetNextDownloadTask(downloadTasks);

        // Assert
        nextDownloadTask.IsSuccess.ShouldBeTrue();
        var nextDownloadTaskId = downloadTasks[4].Children[0].Id;
        nextDownloadTask.Value.Id.ShouldBe(nextDownloadTaskId);
    }
}