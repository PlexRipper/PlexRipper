using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.FileSystem.Contracts;

namespace Reaparr.Application.UnitTests;

public class StartDownloadTaskCommandUnitTests : BaseUnitTest<StartDownloadTaskCommandHandler>
{
    public StartDownloadTaskCommandUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldHaveFailedResult_WhenGivenAnInvalidId()
    {
        // Arrange
        await SetupDatabase(63209);

        // Act
        var result = await Sut.ExecuteAsync(new StartDownloadTaskCommand(Guid.Empty), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Has404NotFoundError().ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldHaveFailedResult_WhenServerIsPausedByUser()
    {
        // Arrange
        await SetupDatabase(
            55109,
            x =>
            {
                x.MovieDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var movieTask = await dbContext.DownloadTaskMovie.FirstAsync(CancellationToken);
        await dbContext
            .PlexServers.Where(x => x.Id == movieTask.PlexServerId)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.IsDownloadsPausedByUser, true), CancellationToken);

        // Act
        var result = await Sut.ExecuteAsync(new StartDownloadTaskCommand(movieTask.Id), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.StartDownloadTaskJob(It.IsAny<DownloadTaskKey>()), Times.Never());
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.StartMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()), Times.Never());
        Mock.VerifyEventPublished(It.IsAny<DownloadTaskUpdatedCommand>, Times.Never());
        Mock.VerifyEventPublished(It.IsAny<CheckDownloadQueueEvent>, Times.Never());
    }

    [Fact]
    public async Task ShouldStartMoveJob_WhenDownloadTaskIsInDownloadFinishedStatus()
    {
        // Arrange
        await SetupDatabase(
            11234,
            x =>
            {
                x.MovieDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var movieDownloadTasks = await dbContext.DownloadTaskMovieFile.AsTracking().ToListAsync(CancellationToken);
        movieDownloadTasks.SetDownloadStatus(DownloadStatus.DownloadFinished);
        await dbContext.SaveChangesAsync(CancellationToken);

        var movieTask = await dbContext.DownloadTaskMovie.FirstAsync(CancellationToken);

        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsDownloadFileMoving(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(false);

        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.StartMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(Result.Ok());

        Mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok());
        Mock.PublishEvent(It.IsAny<CheckDownloadQueueEvent>).Returns(Task.CompletedTask);

        // Act
        var result = await Sut.ExecuteAsync(new StartDownloadTaskCommand(movieTask.Id), CancellationToken);

        // Assert: move job started, download job never touched
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.StartDownloadTaskJob(It.IsAny<DownloadTaskKey>()), Times.Never());
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.IsDownloadFileMoving(It.IsAny<DownloadTaskKey>()), Times.Once());
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.StartMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()), Times.Once());
        Mock.VerifyEventPublished(It.IsAny<DownloadTaskUpdatedCommand>, Times.Once());
        Mock.VerifyEventPublished(It.IsAny<CheckDownloadQueueEvent>, Times.Once());
    }

    [Fact]
    public async Task ShouldStartMoveJob_WhenDownloadTaskIsInMoveErrorStatus()
    {
        // Arrange
        await SetupDatabase(
            55678,
            x =>
            {
                x.MovieDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var movieDownloadTasks = await dbContext.DownloadTaskMovieFile.AsTracking().ToListAsync(CancellationToken);
        movieDownloadTasks.SetDownloadStatus(DownloadStatus.MoveError);
        await dbContext.SaveChangesAsync(CancellationToken);

        var movieTask = await dbContext.DownloadTaskMovie.FirstAsync(CancellationToken);

        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsDownloadFileMoving(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(false);

        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.StartMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(Result.Ok());

        Mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok());
        Mock.PublishEvent(It.IsAny<CheckDownloadQueueEvent>).Returns(Task.CompletedTask);

        // Act
        var result = await Sut.ExecuteAsync(new StartDownloadTaskCommand(movieTask.Id), CancellationToken);

        // Assert: move job retried, download job never touched
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.StartDownloadTaskJob(It.IsAny<DownloadTaskKey>()), Times.Never());
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.IsDownloadFileMoving(It.IsAny<DownloadTaskKey>()), Times.Once());
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.StartMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()), Times.Once());
        Mock.VerifyEventPublished(It.IsAny<DownloadTaskUpdatedCommand>, Times.Once());
        Mock.VerifyEventPublished(It.IsAny<CheckDownloadQueueEvent>, Times.Once());
    }

    [Fact]
    public async Task ShouldNotPauseDownloadTasksInFileTransfer_WhenADownloadTaskIsAlreadyTransferring()
    {
        // Arrange
        await SetupDatabase(
            96318,
            x =>
            {
                x.TvShowDownloadTasksCount = 5;
                x.TvShowSeasonDownloadTasksCount = 2;
                x.TvShowEpisodeCount = 2;
            }
        );

        var dbContext = IDbContext;
        var tvShowDownloadTasks = await dbContext
            .DownloadTaskTvShow.AsTracking()
            .Include(x => x.Children)
                .ThenInclude(x => x.Children)
                    .ThenInclude(x => x.Children)
            .ToListAsync(CancellationToken);

        var alreadyMergingTask = tvShowDownloadTasks.First();
        var pausedMergeTask = tvShowDownloadTasks.Last();

        alreadyMergingTask.SetDownloadStatus(DownloadStatus.Moving);
        pausedMergeTask.SetDownloadStatus(DownloadStatus.MovePaused);
        await dbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsDownloadFileMoving(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(false)
            .Verifiable(Times.Once);

        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.StartMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Once);
        Mock.PublishEvent(It.IsAny<CheckDownloadQueueEvent>).Returns(Task.CompletedTask).Verifiable(Times.Once);

        // Act
        var result = await Sut.ExecuteAsync(new StartDownloadTaskCommand(pausedMergeTask.Id), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldNotPauseTheActiveDownloads_WhenThatActiveDownloadTaskIsStarted()
    {
        // Arrange
        await SetupDatabase(
            31917,
            x =>
            {
                x.TvShowDownloadTasksCount = 5;
                x.TvShowSeasonDownloadTasksCount = 2;
                x.TvShowEpisodeCount = 2;
            }
        );

        var dbContext = IDbContext;
        var tvShowDownloadTasks = await dbContext
            .DownloadTaskTvShow.AsTracking()
            .Include(x => x.Children)
                .ThenInclude(x => x.Children)
                    .ThenInclude(x => x.Children)
            .ToListAsync(CancellationToken);

        tvShowDownloadTasks.SetDownloadStatus(DownloadStatus.Completed);
        var lastDownloadTask = tvShowDownloadTasks.Last();
        lastDownloadTask.SetDownloadStatus(DownloadStatus.Queued);
        await dbContext.SaveChangesAsync(CancellationToken);

        var orderedDownloadTasks = await IDbContext.GetDownloadableChildTasks(
            lastDownloadTask.ToKey(),
            CancellationToken
        );
        orderedDownloadTasks.Count.ShouldBeGreaterThan(0);
        var downloadingTask = orderedDownloadTasks.First();

        await IDbContext
            .DownloadTaskTvShowEpisodeFile.Where(x => x.Id == downloadingTask.Id)
            .ExecuteUpdateAsync(
                p => p.SetProperty(x => x.DownloadStatus, DownloadStatus.Downloading),
                CancellationToken
            );

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.GetCurrentlyDownloadingKeysByServer(It.IsAny<int>()))
            .ReturnsAsync([downloadingTask.ToKey()]);

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.StartDownloadTaskJob(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(Result.Ok());

        Mock.SetupCommand(It.IsAny<PauseDownloadTaskCommand>).ReturnOk();
        Mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok());
        Mock.PublishEvent(It.IsAny<CheckDownloadQueueEvent>).Returns(Task.CompletedTask);

        // Act
        var result = await Sut.ExecuteAsync(new StartDownloadTaskCommand(lastDownloadTask.Id), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var downloadTasks = await IDbContext.GetDownloadableChildTasks(lastDownloadTask.ToKey(), CancellationToken);
        downloadTasks.Count(x => x.DownloadStatus == DownloadStatus.Downloading).ShouldBe(1);
        downloadTasks.Count(x => x.DownloadStatus == DownloadStatus.Queued).ShouldBe(downloadTasks.Count - 1);

        // Verify that the downloading task was not paused as we are starting one that is already downloading
        Mock.VerifyEventPublished(It.IsAny<PauseDownloadTaskCommand>, Times.Never());
        Mock.VerifyEventPublished(It.IsAny<DownloadTaskUpdatedCommand>, Times.Once());
        Mock.VerifyEventPublished(It.IsAny<CheckDownloadQueueEvent>, Times.Once());
    }

    [Fact]
    public async Task ShouldPauseTheActiveDownloads_WhenAnotherDownloadTaskIsStarted()
    {
        // Arrange
        await SetupDatabase(
            76276,
            x =>
            {
                x.TvShowDownloadTasksCount = 5;
                x.TvShowSeasonDownloadTasksCount = 2;
                x.TvShowEpisodeCount = 2;
            }
        );

        var dbContext = IDbContext;
        var tvShowDownloadTasks = await dbContext
            .DownloadTaskTvShow.AsTracking()
            .Include(x => x.Children)
                .ThenInclude(x => x.Children)
                    .ThenInclude(x => x.Children)
            .ToListAsync(CancellationToken.None);

        tvShowDownloadTasks.SetDownloadStatus(DownloadStatus.Completed);
        var lastDownloadTask = tvShowDownloadTasks.Last();
        lastDownloadTask.SetDownloadStatus(DownloadStatus.Queued);
        await dbContext.SaveChangesAsync(CancellationToken);

        var orderedDownloadTasks = await IDbContext.GetDownloadableChildTasks(
            lastDownloadTask.ToKey(),
            CancellationToken
        );
        orderedDownloadTasks.Count.ShouldBeGreaterThan(1);
        var downloadingTask = orderedDownloadTasks[1];

        await IDbContext
            .DownloadTaskTvShowEpisodeFile.Where(x => x.Id == downloadingTask.Id)
            .ExecuteUpdateAsync(
                p => p.SetProperty(x => x.DownloadStatus, DownloadStatus.Downloading),
                CancellationToken
            );

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.GetCurrentlyDownloadingKeysByServer(It.IsAny<int>()))
            .ReturnsAsync([downloadingTask.ToKey()]);

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.StartDownloadTaskJob(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(Result.Ok());

        Mock.SetupCommand(It.IsAny<PauseDownloadTaskCommand>)
            .ReturnsAsync(
                (PauseDownloadTaskCommand command, CancellationToken ct) =>
                {
                    var key = IDbContext.GetDownloadTaskKeyAsync(command.DownloadTaskGuid, ct).Result;
                    key.ShouldNotBeNull();
                    IDbContext.SetDownloadStatus(key, DownloadStatus.Queued).Wait(ct);
                    return Result.Ok();
                }
            );
        Mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok());
        Mock.PublishEvent(It.IsAny<CheckDownloadQueueEvent>).Returns(Task.CompletedTask);

        // Act
        var result = await Sut.ExecuteAsync(new StartDownloadTaskCommand(lastDownloadTask.Id), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var downloadTasks = await IDbContext.GetDownloadableChildTasks(lastDownloadTask.ToKey(), CancellationToken);
        downloadTasks.Count(x => x.DownloadStatus == DownloadStatus.Downloading).ShouldBe(1);
        downloadTasks.Count(x => x.DownloadStatus == DownloadStatus.Queued).ShouldBe(downloadTasks.Count - 1);

        // Verify that the downloading task was not paused as we are starting one that is already downloading
        Mock.VerifyEventPublished(It.IsAny<PauseDownloadTaskCommand>, Times.Once());
        Mock.VerifyEventPublished(It.IsAny<DownloadTaskUpdatedCommand>, Times.Once());
        Mock.VerifyEventPublished(It.IsAny<CheckDownloadQueueEvent>, Times.Once());
    }
}
