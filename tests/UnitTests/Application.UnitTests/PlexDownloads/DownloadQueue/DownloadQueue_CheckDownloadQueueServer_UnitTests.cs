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
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        // Arrange
        // Set up a task that's actually downloading in the database
        var dbContext = IDbContext;
        var downloadTasks = await dbContext
            .DownloadTaskMovie.AsTracking()
            .Where(x => x.PlexServerId == 1)
            .IncludeAll()
            .ToListAsync(CancellationToken);

        var firstMovieDownloadTask = downloadTasks[0];
        await dbContext
            .DownloadTaskMovieFile.Where(x => x.Id == firstMovieDownloadTask.Children.First().Id)
            .ExecuteUpdateAsync(
                p => p.SetProperty(x => x.DownloadStatus, DownloadStatus.Downloading),
                CancellationToken
            );

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
    public async Task ShouldNotStartDownloads_WhenServerIsPausedByUser()
    {
        // Arrange
        await SetupDatabase(
            70112,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 2;
                config.MovieDownloadTasksCount = 2;
            }
        );

        var dbContext = IDbContext;
        var server = await dbContext.PlexServers.AsTracking().FirstAsync(CancellationToken);
        server.IsDownloadsPausedByUser = true;
        await dbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.StartDownloadTaskJob(It.IsAny<DownloadTaskKey>()))
            .ReturnOk()
            .Verifiable(Times.Never);

        // Act
        var result = await Sut.CheckDownloadQueueServer(server.Id);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeNull();
        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.StartDownloadTaskJob(It.IsAny<DownloadTaskKey>()), Times.Never);
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

    [Fact]
    public async Task ShouldStartNextDownloadTask_WhenJobIsStillRunningButDatabaseStatusIsDownloadFinished()
    {
        // Arrange
        // This test simulates the race condition where a download job has finished and updated
        // the database status to DownloadFinished, but the Quartz job is still in the process
        // of cleaning up and hasn't been removed from the scheduler yet.
        await SetupDatabase(
            88234,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 3;
                config.MovieDownloadTasksCount = 3;
            }
        );

        var dbContext = IDbContext;
        var downloadTasks = await dbContext
            .DownloadTaskMovie.AsTracking()
            .Where(x => x.PlexServerId == 1)
            .IncludeAll()
            .ToListAsync(CancellationToken);

        // Set the first task to DownloadFinished (simulating a just-completed download)
        // We set this on the file-level task (child) which is what actually gets downloaded
        var firstMovieDownloadTask = downloadTasks[0];
        var firstFileTaskId = firstMovieDownloadTask.Children.First().Id;
        await dbContext
            .DownloadTaskMovieFile.Where(x => x.Id == firstFileTaskId)
            .ExecuteUpdateAsync(
                p => p.SetProperty(x => x.DownloadStatus, DownloadStatus.DownloadFinished),
                CancellationToken
            );

        // Mock the scheduler to return true (job still running - race condition)
        Mock.Mock<IDownloadTaskScheduler>().Setup(x => x.IsServerDownloading(It.IsAny<int>())).ReturnsAsync(true);

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.StartDownloadTaskJob(It.IsAny<DownloadTaskKey>()))
            .ReturnOk()
            .Verifiable(Times.Once);

        // Act
        var result = await Sut.CheckDownloadQueueServer(1);

        // Assert
        // Should succeed and start the next queued task (any queued task that's not the finished one)
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldNotBe(firstFileTaskId);
        result.Value.DownloadStatus.ShouldBe(DownloadStatus.Queued);

        // Verify that StartDownloadTaskJob was called
        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.StartDownloadTaskJob(It.IsAny<DownloadTaskKey>()), Times.Once);
    }

    [Fact]
    public async Task ShouldNotStartNextDownloadTask_WhenJobIsStillRunningAndDatabaseStatusIsDownloading()
    {
        // Arrange
        // This test verifies that we still block when a task is actively downloading
        await SetupDatabase(
            66451,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 3;
                config.MovieDownloadTasksCount = 3;
            }
        );

        var dbContext = IDbContext;
        var downloadTasks = await dbContext
            .DownloadTaskMovie.AsTracking()
            .Where(x => x.PlexServerId == 1)
            .IncludeAll()
            .ToListAsync(CancellationToken);

        // Set the first task to Downloading (actively downloading)
        var firstMovieDownloadTask = downloadTasks[0];
        await dbContext
            .DownloadTaskMovieFile.Where(x => x.Id == firstMovieDownloadTask.Children.First().Id)
            .ExecuteUpdateAsync(
                p => p.SetProperty(x => x.DownloadStatus, DownloadStatus.Downloading),
                CancellationToken
            );

        // Mock the scheduler to return true (job is running)
        Mock.Mock<IDownloadTaskScheduler>().Setup(x => x.IsServerDownloading(It.IsAny<int>())).ReturnsAsync(true);

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.StartDownloadTaskJob(It.IsAny<DownloadTaskKey>()))
            .ReturnOk()
            .Verifiable(Times.Never);

        // Act
        var result = await Sut.CheckDownloadQueueServer(1);

        // Assert
        // Should fail because there's an active download
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);

        // Verify that StartDownloadTaskJob was NOT called
        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.StartDownloadTaskJob(It.IsAny<DownloadTaskKey>()), Times.Never);
    }
}
