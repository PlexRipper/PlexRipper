using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application.UnitTests;

public class DownloadQueueCheckDownloadQueueUnitTests : BaseUnitTest<DownloadQueue>
{
    public DownloadQueueCheckDownloadQueueUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldHaveNoUpdates_WhenGivenAnEmptyList()
    {
        // Arrange
        await SetupDatabase(54691);

        // Act
        Sut.Setup();
        var result = await Sut.CheckDownloadQueue([]);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ShouldHaveNoStartCommands_WhenServerIsAlreadyDownloading()
    {
        await SetupDatabase(
            78970,
            config =>
            {
                config.PlexServerCount = 1;
            }
        );

        // Arrange
        Mock.Mock<IDownloadTaskScheduler>().Setup(x => x.StartDownloadTaskJob(It.IsAny<DownloadTaskKey>())).ReturnOk();
        Mock.Mock<IDownloadTaskScheduler>().Setup(x => x.IsServerDownloading(It.IsAny<int>())).ReturnsAsync(true);

        // Act
        Sut.Setup();
        var result = await Sut.CheckDownloadQueueServer(1);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ShouldHaveNoStartCommands_WhenATaskIsAlreadyDownloading()
    {
        // Arrange
        await SetupDatabase(
            97870,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 10;
                config.MovieDownloadTasksCount = 5;
            }
        );

        var dbContext = IDbContext;
        var downloadTasks = await dbContext
            .DownloadTaskMovie.AsTracking()
            .Where(x => x.PlexServerId == 1)
            .IncludeAll()
            .ToListAsync(CancellationToken);
        Mock.Mock<IDownloadTaskScheduler>().Setup(x => x.StartDownloadTaskJob(It.IsAny<DownloadTaskKey>())).ReturnOk();
        Mock.Mock<IDownloadTaskScheduler>().Setup(x => x.IsServerDownloading(It.IsAny<int>())).ReturnsAsync(false);

        var startedDownloadTask = downloadTasks[0];
        startedDownloadTask.SetDownloadStatus(DownloadStatus.Downloading);
        await dbContext.SaveChangesAsync(CancellationToken);

        // Act
        var result = await Sut.CheckDownloadQueueServer(1);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeNull();
    }

    [Fact]
    public async Task ShouldHaveOneDownloadTaskStarted_WhenGivenMovieDownloadTasks()
    {
        // Arrange
        await SetupDatabase(
            5000,
            config =>
            {
                config.MovieCount = 2;
                config.MovieDownloadTasksCount = 2;
            }
        );

        var downloadTasks = await IDbContext.GetAllDownloadTasksByServerAsync(cancellationToken: CancellationToken);
        Mock.Mock<IDownloadTaskScheduler>().Setup(x => x.StartDownloadTaskJob(It.IsAny<DownloadTaskKey>())).ReturnOk();
        Mock.Mock<IDownloadTaskScheduler>().Setup(x => x.IsServerDownloading(It.IsAny<int>())).ReturnsAsync(false);

        // Act
        var result = await Sut.CheckDownloadQueueServer(downloadTasks[0].PlexServerId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var startedDownloadTask = downloadTasks[0].Children[0];
        result.Value.Id.ShouldBe(startedDownloadTask.Id);
    }

    [Fact]
    public async Task ShouldHaveNextQueuedDownloadTask_WhenGivenAMovieDownloadTasksWithCompleted()
    {
        // Arrange
        await SetupDatabase(
            45430,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 10;
                config.MovieDownloadTasksCount = 5;
            }
        );

        var downloadTasks = await IDbContext.GetAllDownloadTasksByServerAsync(
            asTracking: true,
            cancellationToken: CancellationToken
        );
        downloadTasks[0].SetDownloadStatus(DownloadStatus.Completed);
        await IDbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IDownloadTaskScheduler>().Setup(x => x.StartDownloadTaskJob(It.IsAny<DownloadTaskKey>())).ReturnOk();
        Mock.Mock<IDownloadTaskScheduler>().Setup(x => x.IsServerDownloading(It.IsAny<int>())).ReturnsAsync(false);

        // Act
        var result = await Sut.CheckDownloadQueueServer(downloadTasks[0].PlexServerId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        downloadTasks[0].Children[0].DownloadTaskType.ShouldBe(DownloadTaskType.MovieData);
        result.Value.Id.ShouldBe(downloadTasks[0].Children[0].Id);
    }

    [Fact]
    public async Task ShouldHaveNextQueuedDownloadTask_WhenGivenATvShowsDownloadTasksWithCompleted()
    {
        // Arrange
        await SetupDatabase(
            25225,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.TvShowCount = 10;
                config.TvShowDownloadTasksCount = 2;
                config.TvShowSeasonDownloadTasksCount = 2;
                config.TvShowEpisodeDownloadTasksCount = 2;
            }
        );
        var downloadTasks = await IDbContext.GetAllDownloadTasksByServerAsync(
            asTracking: true,
            cancellationToken: CancellationToken
        );
        downloadTasks[0].SetDownloadStatus(DownloadStatus.Completed);
        await IDbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IDownloadTaskScheduler>().Setup(x => x.StartDownloadTaskJob(It.IsAny<DownloadTaskKey>())).ReturnOk();
        Mock.Mock<IDownloadTaskScheduler>().Setup(x => x.IsServerDownloading(It.IsAny<int>())).ReturnsAsync(false);

        // Act
        var result = await Sut.CheckDownloadQueueServer(1);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(downloadTasks[0].Children[0].Children[0].Children[0].Id);
    }
}
