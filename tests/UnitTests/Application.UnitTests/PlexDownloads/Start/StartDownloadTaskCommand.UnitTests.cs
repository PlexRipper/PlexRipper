using Application.Contracts;
using Data.Contracts;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application.UnitTests;

public class StartDownloadTaskCommandUnitTests : BaseUnitTest<StartDownloadTaskCommandHandler>
{
    public StartDownloadTaskCommandUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldHaveFailedResult_WhenGivenAnInvalidId()
    {
        // Arrange
        await SetupDatabase();

        // Act
        var result = await _sut.Handle(new StartDownloadTaskCommand(Guid.Empty), CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Has404NotFoundError().ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenNoDownloadTasksCanBeStarted()
    {
        // Arrange
        await SetupDatabase(x =>
        {
            x.TvShowDownloadTasksCount = 5;
            x.TvShowSeasonDownloadTasksCount = 2;
            x.TvShowEpisodeCount = 2;
        });

        var dbContext = IDbContext;
        var tvShowDownloadTasks = await dbContext
            .DownloadTaskTvShow.AsTracking()
            .Include(x => x.Children)
            .ThenInclude(x => x.Children)
            .ThenInclude(x => x.Children)
            .ToListAsync();

        tvShowDownloadTasks.SetDownloadStatus(DownloadStatus.Completed);
        var lastDownloadTask = tvShowDownloadTasks.Last();
        lastDownloadTask.SetDownloadStatus(DownloadStatus.Paused);
        await dbContext.SaveChangesAsync();

        mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.GetCurrentlyDownloadingKeysByServer(It.IsAny<int>()))
            .ReturnsAsync([]);

        mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.StartDownloadTaskJob(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(Result.Ok());

        mock.SetupMediator(It.IsAny<DownloadTaskUpdatedNotification>).Returns(Task.CompletedTask);
        mock.PublishMediator(It.IsAny<CheckDownloadQueueNotification>).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(new StartDownloadTaskCommand(lastDownloadTask.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var downloadTasks = await IDbContext.GetDownloadableChildTasks(lastDownloadTask.ToKey());
        for (var i = 0; i < downloadTasks.Count; i++)
        {
            if (i > 0)
            {
                downloadTasks[i].DownloadStatus.ShouldBe(DownloadStatus.Queued);
                continue;
            }

            downloadTasks[i].DownloadStatus.ShouldBe(DownloadStatus.Downloading);
        }

        mock.VerifyMediator(It.IsAny<DownloadTaskUpdatedNotification>, Times.Once());
        mock.VerifyMediator(It.IsAny<CheckDownloadQueueNotification>, Times.Once());
    }

    [Fact]
    public async Task ShouldNotPauseTheActiveDownloads_WhenThatActiveDownloadTaskIsStarted()
    {
        // Arrange
        await SetupDatabase(x =>
        {
            x.TvShowDownloadTasksCount = 5;
            x.TvShowSeasonDownloadTasksCount = 2;
            x.TvShowEpisodeCount = 2;
        });

        var dbContext = IDbContext;
        var tvShowDownloadTasks = await dbContext
            .DownloadTaskTvShow.AsTracking()
            .Include(x => x.Children)
            .ThenInclude(x => x.Children)
            .ThenInclude(x => x.Children)
            .ToListAsync();

        tvShowDownloadTasks.SetDownloadStatus(DownloadStatus.Completed);
        var lastDownloadTask = tvShowDownloadTasks.Last();
        lastDownloadTask.SetDownloadStatus(DownloadStatus.Queued);
        var downloadingTask = lastDownloadTask.Children[0].Children[0].Children[0];
        downloadingTask.DownloadStatus = DownloadStatus.Downloading;
        await dbContext.SaveChangesAsync();

        mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.GetCurrentlyDownloadingKeysByServer(It.IsAny<int>()))
            .ReturnsAsync([downloadingTask.ToKey()]);

        mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.StartDownloadTaskJob(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(Result.Ok());

        mock.SetupMediator(It.IsAny<PauseDownloadTaskCommand>).ReturnOk();
        mock.SetupMediator(It.IsAny<DownloadTaskUpdatedNotification>).Returns(Task.CompletedTask);
        mock.PublishMediator(It.IsAny<CheckDownloadQueueNotification>).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(new StartDownloadTaskCommand(lastDownloadTask.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var downloadTasks = await IDbContext.GetDownloadableChildTasks(lastDownloadTask.ToKey());
        for (var i = 0; i < downloadTasks.Count; i++)
        {
            if (i > 0)
            {
                downloadTasks[i].DownloadStatus.ShouldBe(DownloadStatus.Queued);
                continue;
            }

            downloadTasks[i].DownloadStatus.ShouldBe(DownloadStatus.Downloading);
        }

        // Verify that the downloading task was not paused as we are starting one that is already downloading
        mock.VerifyMediator(It.IsAny<PauseDownloadTaskCommand>, Times.Never());
        mock.VerifyMediator(It.IsAny<DownloadTaskUpdatedNotification>, Times.Once());
        mock.VerifyMediator(It.IsAny<CheckDownloadQueueNotification>, Times.Once());
    }

    [Fact]
    public async Task ShouldPauseTheActiveDownloads_WhenAnotherDownloadTaskIsStarted()
    {
        // Arrange
        await SetupDatabase(x =>
        {
            x.TvShowDownloadTasksCount = 5;
            x.TvShowSeasonDownloadTasksCount = 2;
            x.TvShowEpisodeCount = 2;
        });

        var dbContext = IDbContext;
        var tvShowDownloadTasks = await dbContext
            .DownloadTaskTvShow.AsTracking()
            .Include(x => x.Children)
            .ThenInclude(x => x.Children)
            .ThenInclude(x => x.Children)
            .ToListAsync();

        tvShowDownloadTasks.SetDownloadStatus(DownloadStatus.Completed);
        var lastDownloadTask = tvShowDownloadTasks.Last();
        lastDownloadTask.SetDownloadStatus(DownloadStatus.Queued);
        var downloadingTask = lastDownloadTask.Children[0].Children[1].Children[0];
        downloadingTask.DownloadStatus = DownloadStatus.Downloading;
        await dbContext.SaveChangesAsync();

        mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.GetCurrentlyDownloadingKeysByServer(It.IsAny<int>()))
            .ReturnsAsync([downloadingTask.ToKey()]);

        mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.StartDownloadTaskJob(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(Result.Ok());

        mock.SetupMediator(It.IsAny<PauseDownloadTaskCommand>)
            .ReturnsAsync(
                (PauseDownloadTaskCommand command, CancellationToken ct) =>
                {
                    var key = IDbContext.GetDownloadTaskKeyAsync(command.DownloadTaskGuid, ct).Result;
                    key.ShouldNotBeNull();
                    IDbContext.SetDownloadStatus(key, DownloadStatus.Paused).Wait(ct);
                    return Result.Ok();
                }
            );
        mock.SetupMediator(It.IsAny<DownloadTaskUpdatedNotification>).Returns(Task.CompletedTask);
        mock.PublishMediator(It.IsAny<CheckDownloadQueueNotification>).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(new StartDownloadTaskCommand(lastDownloadTask.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var downloadTasks = await IDbContext.GetDownloadableChildTasks(lastDownloadTask.ToKey());
        for (var i = 0; i < downloadTasks.Count; i++)
        {
            if (i > 0)
            {
                downloadTasks[i].DownloadStatus.ShouldBe(DownloadStatus.Queued);
                continue;
            }

            downloadTasks[i].DownloadStatus.ShouldBe(DownloadStatus.Downloading);
        }

        // Verify that the downloading task was not paused as we are starting one that is already downloading
        mock.VerifyMediator(It.IsAny<PauseDownloadTaskCommand>, Times.Once());
        mock.VerifyMediator(It.IsAny<DownloadTaskUpdatedNotification>, Times.Once());
        mock.VerifyMediator(It.IsAny<CheckDownloadQueueNotification>, Times.Once());
    }
}
