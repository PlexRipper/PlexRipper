using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ByteSizeLib;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.FileSystem.Contracts;
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

        mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);
        mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Once);
        mock.SetupCommand(It.IsAny<MoveFileWithResumeCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Never);

        // Act
        var command = new MoveDownloadFileFromFileTaskCommand(downloadTask.ToKey(), progress);
        var result = await _sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        ;
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
            fs.AddFile(downloadFileTask.FilePath, new MockFileData(content));
            fs.AddFile(downloadFileTask.DestinationFilePath, new MockFileData(new byte[0]));
        });

        downloadFileTask.DataTotal = content.LongLength;
        await IDbContext.SaveChangesAsync(CancellationToken);

        mock.Mock<IDownloadManagerSettings>()
            .Setup(x => x.KeepCompletedInDownloadFolder)
            .Returns(false)
            .Verifiable(Times.Once);

        // Will publish if an error occurs
        mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);

        mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.AtLeastOnce);
        mock.SetupCommand(It.IsAny<MoveFileWithResumeCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Once);

        // Act
        var command = new MoveDownloadFileFromFileTaskCommand(downloadFileTask.ToKey(), progress);
        var result = await _sut.ExecuteAsync(command, cancellationTokenSource.Token);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        progressList.Any().ShouldBeTrue();

        var fileTaskPaused = await IDbContext.GetDownloadTaskFileAsync(downloadFileTask.ToKey(), CancellationToken);
        fileTaskPaused.ShouldNotBeNull();
        fileTaskPaused.CurrentFileTransferBytesOffset.ShouldBeGreaterThanOrEqualTo(0);

        // Source should remain since operation paused before completion
        var file = mock.Create<IFile>();
        file.Exists(downloadFileTask.FilePath).ShouldBeTrue();
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

        mock.Mock<IDownloadManagerSettings>().Setup(x => x.KeepCompletedInDownloadFolder).Returns(false);

        var content = new byte[fileSizeInMb * 1024 * 1024];
        new Random(3).NextBytes(content);

        SetupFileSystem(fs =>
        {
            fs.AddFile(downloadFileTask.FilePath, new MockFileData(content));
            fs.AddFile(downloadFileTask.DestinationFilePath, new MockFileData([]));
        });

        downloadFileTask.DataTotal = content.LongLength;
        await IDbContext.SaveChangesAsync(CancellationToken);

        mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);
        mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.AtLeastOnce);
        mock.SetupCommand(It.IsAny<MoveFileWithResumeCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Once);
        mock.Mock<IDownloadManagerSettings>().Setup(x => x.KeepCompletedInDownloadFolder).Returns(false);

        // Act
        var command = new MoveDownloadFileFromFileTaskCommand(downloadFileTask.ToKey(), progress);
        var result = await _sut.ExecuteAsync(command, cancellationTokenSource.Token);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        progressList.Any().ShouldBeTrue();

        var fileTaskPaused = await IDbContext.GetDownloadTaskFileAsync(downloadFileTask.ToKey(), CancellationToken);
        fileTaskPaused.ShouldNotBeNull();
        fileTaskPaused.CurrentFileTransferBytesOffset.ShouldBeGreaterThanOrEqualTo(0);
    }
}
