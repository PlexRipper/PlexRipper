using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ByteSizeLib;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application.UnitTests;

public class MoveDownloadFileFromFileTaskCommandUnitTests : BaseUnitTest<MoveDownloadFileFromFileTaskCommandHandler>
{
    public MoveDownloadFileFromFileTaskCommandUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenDirectoryNameIsEmpty()
    {
        // Arrange
        await SetupDatabase(
            10001,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
                config.DownloadWorkerTasks = 1;
            }
        );

        var downloadTask = await IDbContext
            .DownloadTaskTvShowEpisodeFile.Include(x => x.DownloadWorkerTasks)
            .FirstOrDefaultAsync(CancellationToken);
        downloadTask.ShouldNotBeNull();

        var progress = new Subject<IDownloadFileTransferProgress>();

        SetupFileSystem();

        Mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);
        Mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Once);
        Mock.SetupCommand(It.IsAny<MoveFileWithResumeCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Never);

        // Act
        var command = new MoveDownloadFileFromFileTaskCommand(downloadTask.ToKey(), progress);
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldBeAbleToPauseTheDownloadTask_WhenCancellationTokenIsCalled()
    {
        // Arrange
        await SetupDatabase(
            52223,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
                config.DownloadWorkerTasks = 4;
            }
        );

        var downloadFileTask = await IDbContext
            .DownloadTaskTvShowEpisodeFile.Include(x => x.DownloadWorkerTasks)
            .FirstOrDefaultAsync(CancellationToken);
        downloadFileTask.ShouldNotBeNull();

        var fileSizeInMb = 10;
        var progress = new Subject<IDownloadFileTransferProgress>();

        var cancellationTokenSource = new CancellationTokenSource();
        var progressList = new List<IDownloadFileTransferProgress>();
        progress
            .AsObservable()
            .Subscribe(x =>
            {
                progressList.Add(x);

                // Cancel shortly after the transfer starts
                if (
                    x.FileDataTransferred > (long)ByteSize.FromMebiBytes(1).Bytes
                    && !cancellationTokenSource.Token.IsCancellationRequested
                )
                {
                    cancellationTokenSource.CancelAsync();
                }
            });

        var content = new byte[fileSizeInMb * 1024 * 1024];
        new Random(2).NextBytes(content);

        SetupFileSystem(fs =>
        {
            fs.AddFile(downloadFileTask.DownloadFilePath, new MockFileData(content));
            fs.AddFile(downloadFileTask.DestinationFilePath, new MockFileData([]));
        });

        downloadFileTask.DataTotal = content.LongLength;
        await IDbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IDownloadManagerSettings>()
            .Setup(x => x.KeepCompletedInDownloadFolder)
            .Returns(false)
            .Verifiable(Times.Once);

        // Will publish if an error occurs
        Mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);

        Mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.AtLeastOnce);
        Mock.SetupCommand(It.IsAny<MoveFileWithResumeCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Once);

        // Act
        var command = new MoveDownloadFileFromFileTaskCommand(downloadFileTask.ToKey(), progress);
        var result = await Sut.ExecuteAsync(command, cancellationTokenSource.Token);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        progressList.Any().ShouldBeTrue();

        var fileTaskPaused = await IDbContext.GetDownloadTaskFileAsync(downloadFileTask.ToKey(), CancellationToken);
        fileTaskPaused.ShouldNotBeNull();
        fileTaskPaused.CurrentFileTransferBytesOffset.ShouldBeGreaterThanOrEqualTo(0);

        // Source should remain since operation paused before completion
        var file = Mock.Create<IFile>();
        file.Exists(downloadFileTask.DownloadFilePath).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldBeAbleToResumeAndFinish_WhenPreviousFileTaskHasBeenPaused()
    {
        // Arrange
        await SetupDatabase(
            52223,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
                config.DownloadWorkerTasks = 4;
            }
        );

        var dbContext = IDbContext;
        var downloadFileTask = await dbContext
            .DownloadTaskTvShowEpisodeFile.AsTracking()
            .Include(x => x.DownloadWorkerTasks)
            .FirstOrDefaultAsync(CancellationToken);
        downloadFileTask.ShouldNotBeNull();

        downloadFileTask.CurrentFileTransferBytesOffset = 2348;
        await dbContext.SaveChangesAsync(CancellationToken);

        var fileSizeInMb = 10;
        var progress = new Subject<IDownloadFileTransferProgress>();

        var cancellationTokenSource = new CancellationTokenSource();
        var progressList = new List<IDownloadFileTransferProgress>();
        progress.AsObservable().Subscribe(x => progressList.Add(x));

        Mock.Mock<IDownloadManagerSettings>().Setup(x => x.KeepCompletedInDownloadFolder).Returns(false);

        var content = new byte[fileSizeInMb * 1024 * 1024];
        new Random(3).NextBytes(content);

        SetupFileSystem(fs =>
        {
            fs.AddFile(downloadFileTask.DownloadFilePath, new MockFileData(content));
            fs.AddFile(downloadFileTask.DestinationFilePath, new MockFileData([]));
        });

        downloadFileTask.DataTotal = content.LongLength;
        await IDbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);
        Mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.AtLeastOnce);
        Mock.SetupCommand(It.IsAny<MoveFileWithResumeCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Once);
        Mock.Mock<IDownloadManagerSettings>().Setup(x => x.KeepCompletedInDownloadFolder).Returns(false);

        // Act
        var command = new MoveDownloadFileFromFileTaskCommand(downloadFileTask.ToKey(), progress);
        var result = await Sut.ExecuteAsync(command, cancellationTokenSource.Token);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        progressList.Any().ShouldBeTrue();

        var fileTaskPaused = await IDbContext.GetDownloadTaskFileAsync(downloadFileTask.ToKey(), CancellationToken);
        fileTaskPaused.ShouldNotBeNull();
        fileTaskPaused.CurrentFileTransferBytesOffset.ShouldBeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task ShouldRenameAndComplete_WhenKeepCompletedInDownloadsIsTrue()
    {
        // Arrange
        await SetupDatabase(
            334455,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
                config.DownloadWorkerTasks = 3;
            }
        );

        var downloadFileTask = await IDbContext
            .DownloadTaskTvShowEpisodeFile.Include(x => x.DownloadWorkerTasks)
            .FirstOrDefaultAsync(CancellationToken);
        downloadFileTask.ShouldNotBeNull();

        var progress = new Subject<IDownloadFileTransferProgress>();

        var content = new byte[2 * 1024 * 1024];
        new Random(11).NextBytes(content);

        SetupFileSystem(fs =>
        {
            // Source exists with .reaptemp suffix
            fs.AddFile(downloadFileTask.DownloadFilePath, new MockFileData(content));
        });

        downloadFileTask.DataTotal = content.LongLength;
        await IDbContext.SaveChangesAsync(CancellationToken);

        // Keep in downloads triggers rename flow, not MoveFileWithResume
        Mock.Mock<IDownloadManagerSettings>()
            .Setup(x => x.KeepCompletedInDownloadFolder)
            .Returns(true)
            .Verifiable(Times.Once);

        Mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);

        Mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.AtLeastOnce);
        Mock.SetupCommand(It.IsAny<MoveFileWithResumeCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Never);

        // Act
        var command = new MoveDownloadFileFromFileTaskCommand(downloadFileTask.ToKey(), progress);
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var file = Mock.Create<IFile>();
        file.Exists(downloadFileTask.DownloadFilePath).ShouldBeFalse();
        file.Exists(downloadFileTask.DownloadFilePath.RemoveReapTempSuffix()).ShouldBeTrue();

        var after = await IDbContext.GetDownloadTaskFileAsync(downloadFileTask.ToKey(), CancellationToken);
        after.ShouldNotBeNull();
        after.DownloadStatus.ShouldBe(DownloadStatus.MoveFinished);
        after.FileDataTransferred.ShouldBe(after.DataTotal);
        after.CurrentFileTransferBytesOffset.ShouldBe(after.DataTotal);
    }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenDownloadTaskKeyDoesNotExist()
    {
        // Arrange
        await SetupDatabase(
            778899,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
                config.DownloadWorkerTasks = 1;
            }
        );

        var existing = await IDbContext
            .DownloadTaskTvShowEpisodeFile.AsNoTracking()
            .FirstOrDefaultAsync(CancellationToken);
        existing.ShouldNotBeNull();

        var missingKey = new DownloadTaskKey
        {
            Type = existing.DownloadTaskType,
            Id = Guid.NewGuid(),
            PlexServerId = existing.PlexServerId,
            PlexLibraryId = existing.PlexLibraryId,
        };

        var progress = new Subject<IDownloadFileTransferProgress>();
        SetupFileSystem();

        Mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);

        Mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Never);
        Mock.SetupCommand(It.IsAny<MoveFileWithResumeCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Never);

        // Act
        var result = await Sut.ExecuteAsync(
            new MoveDownloadFileFromFileTaskCommand(missingKey, progress),
            CancellationToken
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnFailedResultAndPublishNotification_WhenSourceFileDoesNotExist()
    {
        // Arrange
        await SetupDatabase(
            991122,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
                config.DownloadWorkerTasks = 2;
            }
        );

        var downloadFileTask = await IDbContext
            .DownloadTaskTvShowEpisodeFile.Include(x => x.DownloadWorkerTasks)
            .FirstOrDefaultAsync(CancellationToken);
        downloadFileTask.ShouldNotBeNull();

        var progress = new Subject<IDownloadFileTransferProgress>();

        // No source file added
        SetupFileSystem();

        Mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Once);
        Mock.SetupCommand(It.IsAny<MoveFileWithResumeCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Never);

        // Act
        var result = await Sut.ExecuteAsync(
            new MoveDownloadFileFromFileTaskCommand(downloadFileTask.ToKey(), progress),
            CancellationToken
        );

        // Assert
        result.IsFailed.ShouldBeTrue();

        var after = await IDbContext.GetDownloadTaskFileAsync(downloadFileTask.ToKey(), CancellationToken);
        after.ShouldNotBeNull();
        after.DownloadStatus.ShouldBe(DownloadStatus.MoveError);
    }

    [Fact]
    public async Task ShouldReturnFailedResultAndPublishNotification_WhenMoveWithResumeFails()
    {
        // Arrange
        await SetupDatabase(
            112233,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
                config.DownloadWorkerTasks = 3;
            }
        );

        var downloadFileTask = await IDbContext
            .DownloadTaskTvShowEpisodeFile.Include(x => x.DownloadWorkerTasks)
            .FirstOrDefaultAsync(CancellationToken);
        downloadFileTask.ShouldNotBeNull();

        var progress = new Subject<IDownloadFileTransferProgress>();

        var content = new byte[3 * 1024 * 1024];
        new Random(7).NextBytes(content);

        SetupFileSystem(fs =>
        {
            fs.AddFile(downloadFileTask.DownloadFilePath, new MockFileData(content));
        });

        downloadFileTask.DataTotal = content.LongLength;
        await IDbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IDownloadManagerSettings>()
            .Setup(x => x.KeepCompletedInDownloadFolder)
            .Returns(false)
            .Verifiable(Times.Once);

        Mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);

        Mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.AtLeastOnce);
        Mock.SetupCommand(It.IsAny<MoveFileWithResumeCommand>).ReturnsAsync(Result.Fail("boom")).Verifiable(Times.Once);

        // Act
        var result = await Sut.ExecuteAsync(
            new MoveDownloadFileFromFileTaskCommand(downloadFileTask.ToKey(), progress),
            CancellationToken
        );

        // Assert
        result.IsFailed.ShouldBeTrue();

        var file = Mock.Create<IFile>();
        file.Exists(downloadFileTask.DownloadFilePath).ShouldBeTrue();

        var after = await IDbContext.GetDownloadTaskFileAsync(downloadFileTask.ToKey(), CancellationToken);
        after.ShouldNotBeNull();
        after.DownloadStatus.ShouldBe(DownloadStatus.MoveError);
    }
}
