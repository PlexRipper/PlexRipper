using Reaparr.Data.Contracts;

namespace Reaparr.Application.UnitTests;

public class DownloadQueue_GetNextDownloadTask_UnitTests : BaseUnitTest<DownloadQueue>
{
    public DownloadQueue_GetNextDownloadTask_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldHaveNextDownloadTask_WhenAllAreQueued()
    {
        // Arrange
        await SetupDatabase(69402, config => config.TvShowDownloadTasksCount = 5);
        var downloadTasks = await IDbContext.GetAllDownloadTasksByServerAsync(cancellationToken: CancellationToken);

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
        await SetupDatabase(92673, config => config.TvShowDownloadTasksCount = 5);
        var downloadTasks = await IDbContext.GetAllDownloadTasksByServerAsync(
            asTracking: true,
            cancellationToken: CancellationToken
        );
        downloadTasks[0].SetDownloadStatus(DownloadStatus.Completed);
        await IDbContext.SaveChangesAsync(CancellationToken);

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
        await SetupDatabase(48899, config => config.TvShowDownloadTasksCount = 5);

        var downloadTasks = await IDbContext.GetAllDownloadTasksByServerAsync(
            asTracking: true,
            cancellationToken: CancellationToken
        );
        downloadTasks[0].SetDownloadStatus(DownloadStatus.Downloading);
        foreach (var child in downloadTasks[0].Children)
            child.SetDownloadStatus(DownloadStatus.Queued);
        await IDbContext.SaveChangesAsync(CancellationToken);

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
        await SetupDatabase(69598, config => config.TvShowDownloadTasksCount = 5);

        var downloadTasks = await IDbContext.GetAllDownloadTasksByServerAsync(
            asTracking: true,
            cancellationToken: CancellationToken
        );
        downloadTasks[0].SetDownloadStatus(DownloadStatus.Downloading);
        await IDbContext.SaveChangesAsync(CancellationToken);

        // Act
        var nextDownloadTask = _sut.GetNextDownloadTask(downloadTasks);

        // Assert
        nextDownloadTask.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldHaveServerUnreachableDownloadTask_WhenADownloadTaskIsAlreadyDownloading()
    {
        // Arrange
        await SetupDatabase(69598, config => config.TvShowDownloadTasksCount = 5);

        var downloadTasks = await IDbContext.GetAllDownloadTasksByServerAsync(
            asTracking: true,
            cancellationToken: CancellationToken
        );
        downloadTasks[0].SetDownloadStatus(DownloadStatus.ServerUnreachable);
        await IDbContext.SaveChangesAsync(CancellationToken);

        // Act
        var nextDownloadTask = _sut.GetNextDownloadTask(downloadTasks);

        // Assert
        nextDownloadTask.IsSuccess.ShouldBeTrue();
        var nextDownloadTaskId = downloadTasks[0].Children[0].Children[0].Children[0].Id;
        nextDownloadTask.Value.Id.ShouldBe(nextDownloadTaskId);
    }

    [Fact]
    public async Task ShouldHaveNoNextDownloadTask_WhenMovingAndDownloadFinished()
    {
        // Arrange
        await SetupDatabase(61612, config => config.MovieDownloadTasksCount = 5);

        var downloadTasks = await IDbContext.GetAllDownloadTasksByServerAsync(
            asTracking: true,
            cancellationToken: CancellationToken
        );
        downloadTasks[0].SetDownloadStatus(DownloadStatus.Moving);
        downloadTasks[1].SetDownloadStatus(DownloadStatus.DownloadFinished);
        downloadTasks[2].SetDownloadStatus(DownloadStatus.DownloadFinished);
        downloadTasks[3].SetDownloadStatus(DownloadStatus.DownloadFinished);
        downloadTasks[4].SetDownloadStatus(DownloadStatus.DownloadFinished);
        await IDbContext.SaveChangesAsync(CancellationToken);

        // Act
        var nextDownloadTask = _sut.GetNextDownloadTask(downloadTasks);

        // Assert
        nextDownloadTask.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldHaveLastQueuedDownloadTask_WhenMovingQueuedAndDownloadFinished()
    {
        // Arrange
        await SetupDatabase(13297, config => config.MovieDownloadTasksCount = 5);

        var downloadTasks = await IDbContext.GetAllDownloadTasksByServerAsync(
            asTracking: true,
            cancellationToken: CancellationToken
        );
        downloadTasks[0].SetDownloadStatus(DownloadStatus.Moving);
        downloadTasks[1].SetDownloadStatus(DownloadStatus.DownloadFinished);
        downloadTasks[2].SetDownloadStatus(DownloadStatus.DownloadFinished);
        downloadTasks[3].SetDownloadStatus(DownloadStatus.DownloadFinished);
        downloadTasks[4].SetDownloadStatus(DownloadStatus.Queued);
        await IDbContext.SaveChangesAsync(CancellationToken);

        // Act
        var nextDownloadTask = _sut.GetNextDownloadTask(downloadTasks);

        // Assert
        nextDownloadTask.IsSuccess.ShouldBeTrue();
        var nextDownloadTaskId = downloadTasks[4].Children[0].Id;
        nextDownloadTask.Value.Id.ShouldBe(nextDownloadTaskId);
    }
}
