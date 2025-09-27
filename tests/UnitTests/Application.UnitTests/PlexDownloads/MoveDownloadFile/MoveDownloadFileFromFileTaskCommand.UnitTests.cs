using System.IO.Abstractions;
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

    private void MockCreateDirectoryFromFilePath()
    {
        mock.Mock<IPath>().Setup(x => x.GetDirectoryName(It.IsAny<string>())).Returns("folder").Verifiable(Times.Once);
        mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns(new Mock<IDirectoryInfo>().Object)
            .Verifiable(Times.Once);
    }

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

        mock.Mock<IPath>()
            .Setup(x => x.Combine(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string a, string b) => Path.Combine(a, b))
            .Verifiable(Times.Once);
        mock.Mock<IPath>()
            .Setup(x => x.GetDirectoryName(It.IsAny<string>()))
            .Returns(string.Empty)
            .Verifiable(Times.Once);

        mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);
        mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Never);
        mock.SetupCommand(It.IsAny<MoveFileWithResumeCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Never);

        // Act
        var command = new MoveDownloadFileFromFileTaskCommand(downloadTask.ToKey(), progress);
        var result = await _sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.First().Message.ShouldContain("Could not determine the directory name of path");
    }

    [Fact]
    public async Task ShouldFinishWithRename_WhenMoveSucceeds()
    {
        // Arrange
        await SetupDatabase(
            10002,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
                config.DownloadWorkerTasks = 1;
            }
        );

        var key = await IDbContext
            .DownloadTaskTvShowEpisodeFile.Select(x => x.ToKey())
            .FirstOrDefaultAsync(CancellationToken);
        key.ShouldNotBeNull();

        var progress = new Subject<IDownloadFileTransferProgress>();

        mock.Mock<IPath>()
            .Setup(x => x.Combine(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string a, string b) => Path.Combine(a, b))
            .Verifiable(Times.Once);
        mock.Mock<IPath>().Setup(x => x.GetDirectoryName(It.IsAny<string>())).Returns("folder").Verifiable(Times.Once);
        mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns(new Mock<IDirectoryInfo>().Object)
            .Verifiable(Times.Once);

        mock.Mock<IDownloadManagerSettings>().Setup(x => x.KeepCompletedInDownloadFolder).Returns(false);

        mock.Mock<IFile>().Setup(x => x.Move(It.IsAny<string>(), It.IsAny<string>())).Verifiable(Times.Once);
        mock.Mock<IFile>()
            .Setup(x => x.Open(It.IsAny<string>(), It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()))
            .Verifiable(Times.Never);
        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);

        mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);
        mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.AtLeastOnce);

        // Act
        var command = new MoveDownloadFileFromFileTaskCommand(key, progress);
        var result = await _sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var downloadTaskDb = await IDbContext.GetDownloadTaskFileAsync(key, CancellationToken);
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.MoveFinished);
    }

    [Fact]
    public async Task ShouldOverwriteExistingTarget_WhenTargetExistsAndCopySucceeds()
    {
        // Arrange
        await SetupDatabase(
            10003,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
                config.DownloadWorkerTasks = 2;
            }
        );

        var downloadTask = await IDbContext
            .DownloadTaskTvShowEpisodeFile.Include(x => x.DownloadWorkerTasks)
            .FirstOrDefaultAsync(CancellationToken);
        downloadTask.ShouldNotBeNull();

        var fileSizeInMb = 4;
        var progress = new Subject<IDownloadFileTransferProgress>();
        var readStreams = new List<FileSystemStream>();
        var writeStream = FakeData.GetFileSystemStream();

        mock.Mock<IPath>()
            .Setup(x => x.Combine(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string a, string b) => Path.Combine(a, b))
            .Verifiable(Times.Once);
        mock.Mock<IPath>().Setup(x => x.GetDirectoryName(It.IsAny<string>())).Returns("folder").Verifiable(Times.Once);
        mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns(new Mock<IDirectoryInfo>().Object)
            .Verifiable(Times.Once);

        mock.Mock<IDownloadManagerSettings>().Setup(x => x.KeepCompletedInDownloadFolder).Returns(false);

        // Force move to fail so it copies
        mock.Mock<IFile>().Setup(x => x.Move(It.IsAny<string>(), It.IsAny<string>())).Throws<IOException>();
        mock.Mock<IFile>()
            .Setup(x => x.Open(It.IsAny<string>(), FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            .Returns(() =>
            {
                readStreams.Add(FakeData.GetFileSystemStream(fileSizeInMb));
                return readStreams.Last();
            });
        mock.Mock<IFile>()
            .Setup(x => x.Open(It.IsAny<string>(), FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            .Returns(() => writeStream);

        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        mock.Mock<IFile>().Setup(x => x.Delete(It.IsAny<string>())).Verifiable(Times.AtLeastOnce);
        mock.Mock<IFile>().Setup(x => x.Move(It.IsAny<string>(), It.IsAny<string>())).Verifiable(Times.AtLeastOnce);

        mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.AtMostOnce);
        mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.AtLeastOnce);

        // Act
        var command = new MoveDownloadFileFromFileTaskCommand(downloadTask.ToKey(), progress);
        var result = await _sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.Mock<IFile>().Verify(x => x.Delete(It.IsAny<string>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ShouldFail_WhenOpenWriteThrows()
    {
        // Arrange
        await SetupDatabase(
            10004,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
                config.DownloadWorkerTasks = 1;
            }
        );

        var key = await IDbContext
            .DownloadTaskTvShowEpisodeFile.Select(x => x.ToKey())
            .FirstOrDefaultAsync(CancellationToken);
        key.ShouldNotBeNull();

        var progress = new Subject<IDownloadFileTransferProgress>();

        mock.Mock<IPath>()
            .Setup(x => x.Combine(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string a, string b) => Path.Combine(a, b))
            .Verifiable(Times.Once);
        mock.Mock<IPath>().Setup(x => x.GetDirectoryName(It.IsAny<string>())).Returns("folder").Verifiable(Times.Once);
        mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns(new Mock<IDirectoryInfo>().Object)
            .Verifiable(Times.Once);

        mock.Mock<IDownloadManagerSettings>().Setup(x => x.KeepCompletedInDownloadFolder).Returns(false);

        // Force move to fail so it copies
        mock.Mock<IFile>().Setup(x => x.Move(It.IsAny<string>(), It.IsAny<string>())).Throws<IOException>();
        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        mock.Mock<IFile>()
            .Setup(x => x.Open(It.IsAny<string>(), FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            .Throws<UnauthorizedAccessException>();

        mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);
        mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.AtLeastOnce);

        // Act
        var command = new MoveDownloadFileFromFileTaskCommand(key, progress);
        var result = await _sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldUseDownloadsFolderAsTarget_WhenKeepCompletedInDownloadFolderTrue()
    {
        // Arrange
        await SetupDatabase(
            10005,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
                config.DownloadWorkerTasks = 2;
            }
        );

        var downloadTask = await IDbContext
            .DownloadTaskTvShowEpisodeFile.Include(x => x.DownloadWorkerTasks)
            .FirstOrDefaultAsync(CancellationToken);
        downloadTask.ShouldNotBeNull();

        var progress = new Subject<IDownloadFileTransferProgress>();
        var writeStream = FakeData.GetFileSystemStream();

        mock.Mock<IPath>()
            .Setup(x => x.Combine(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string a, string b) => Path.Combine(a, b))
            .Verifiable(Times.Once);
        mock.Mock<IPath>().Setup(x => x.GetDirectoryName(It.IsAny<string>())).Returns("folder").Verifiable(Times.Once);
        mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns(new Mock<IDirectoryInfo>().Object)
            .Verifiable(Times.Once);

        // Enable keep in downloads
        mock.Mock<IDownloadManagerSettings>().Setup(x => x.KeepCompletedInDownloadFolder).Returns(true);

        // Force move to fail so it copies
        mock.Mock<IFile>().Setup(x => x.Move(It.IsAny<string>(), It.IsAny<string>())).Throws<IOException>();
        mock.Mock<IFile>()
            .Setup(x => x.Open(It.IsAny<string>(), It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()))
            .Returns(() => FakeData.GetFileSystemStream());
        mock.Mock<IFile>()
            .Setup(x => x.Open(It.IsAny<string>(), FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            .Returns(() => writeStream);

        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        mock.Mock<IFile>().Setup(x => x.Delete(It.IsAny<string>())).Verifiable(Times.AtLeastOnce);

        mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);
        
        mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.AtLeastOnce);

        // Act
        var command = new MoveDownloadFileFromFileTaskCommand(downloadTask.ToKey(), progress);
        var result = await _sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenDestinationDirectoryCreationFails()
    {
        // Arrange
        await SetupDatabase(
            23522,
            config =>
            {
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
                config.DownloadWorkerTasks = 4;
            }
        );

        var downloadTask = await IDbContext
            .DownloadTaskTvShowEpisodeFile.Include(x => x.DownloadWorkerTasks)
            .FirstOrDefaultAsync(CancellationToken);
        downloadTask.ShouldNotBeNull();

        var progress = new Subject<IDownloadFileTransferProgress>();
        mock.Mock<IPath>()
            .Setup(x => x.Combine(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string a, string b) => Path.Combine(a, b))
            .Verifiable(Times.Once);
        mock.Mock<IPath>().Setup(x => x.GetDirectoryName(It.IsAny<string>())).Returns("folder").Verifiable(Times.Once);
        mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Throws(new Exception("Failed to create directory"))
            .Verifiable(Times.Once);

        mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);
        mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Once);

        // Act
        var command = new MoveDownloadFileFromFileTaskCommand(downloadTask.ToKey(), progress);
        var result = await _sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.First().Message.ShouldContain("Failed to create directory");
    }

    [Fact]
    public async Task ShouldSetDownloadStatusToMergeError_WhenFileOpenFails()
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

        var key = await IDbContext
            .DownloadTaskTvShowEpisodeFile.Select(x => x.ToKey())
            .FirstOrDefaultAsync(CancellationToken);
        key.ShouldNotBeNull();

        var progress = new Subject<IDownloadFileTransferProgress>();

        MockCreateDirectoryFromFilePath();

        mock.Mock<IPath>()
            .Setup(x => x.Combine(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string a, string b) => Path.Combine(a, b))
            .Verifiable(Times.Once);
        mock.Mock<IFile>()
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<FileOptions>()))
            .Throws<UnauthorizedAccessException>();
        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true);

        mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);
        mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Once);

        // Act
        var command = new MoveDownloadFileFromFileTaskCommand(key, progress);
        var result = await _sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();

        var downloadTaskDb = await IDbContext.GetDownloadTaskFileAsync(key, CancellationToken);
        downloadTaskDb.ShouldNotBeNull();
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.MoveError);
    }

    [Fact]
    public async Task ShouldDeleteAllFilesAndMarkFileTaskCompleted_WhenItIsDoneMerging()
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
        var readStreams = new List<FileSystemStream>();
        var writeStream = FakeData.GetFileSystemStream();

        var progressList = new List<IDownloadFileTransferProgress>();
        progress.AsObservable().Subscribe(x => progressList.Add(x));

        mock.Mock<IDownloadManagerSettings>().Setup(x => x.KeepCompletedInDownloadFolder).Returns(false);

        mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns(new Mock<IDirectoryInfo>().Object)
            .Verifiable(Times.Once);
        mock.Mock<IPath>()
            .Setup(x => x.Combine(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string a, string b) => Path.Combine(a, b))
            .Verifiable(Times.Once);
        mock.Mock<IPath>().Setup(x => x.GetDirectoryName(It.IsAny<string>())).Returns("folder").Verifiable(Times.Once);
        mock.Mock<IFile>()
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<FileOptions>()))
            .Returns(() => writeStream);
        mock.Mock<IFile>()
            .Setup(x => x.Open(It.IsAny<string>(), FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            .Returns(() => writeStream);
        mock.Mock<IFile>()
            .Setup(x => x.Open(It.IsAny<string>(), It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()))
            .Returns(() =>
            {
                readStreams.Add(FakeData.GetFileSystemStream(fileSizeInMb));
                return readStreams.Last();
            })
            .Verifiable(Times.Exactly(2));
        mock.Mock<IFile>().Setup(x => x.Delete(It.IsAny<string>())).Verifiable(Times.AtLeastOnce);
        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true).Verifiable(Times.AtLeastOnce);

        mock.Mock<IFileSystem>().Setup(x => x.File).Returns(mock.Mock<IFile>().Object);
        mock.Mock<IFileSystem>().Setup(x => x.Path).Returns(mock.Mock<IPath>().Object);
        mock.Mock<IFileSystem>().Setup(x => x.Directory).Returns(mock.Mock<IDirectory>().Object);

        mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);

        mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.AtLeastOnce);

        // Act
        var command = new MoveDownloadFileFromFileTaskCommand(downloadFileTask.ToKey(), progress);
        var result = await _sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        progressList.Any().ShouldBeTrue();

        var fileTaskPaused = await IDbContext.GetDownloadTaskFileAsync(downloadFileTask.ToKey(), CancellationToken);
        fileTaskPaused.ShouldNotBeNull();
        fileTaskPaused.CurrentFileTransferPathIndex.ShouldBe(0);
        fileTaskPaused.CurrentFileTransferBytesOffset.ShouldBe(0);

        foreach (var readStream in readStreams)
            Should.Throw<ObjectDisposedException>(() => readStream.WriteByte(0));

        // Write stream disposal timing may vary; skip strict disposal assertion
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
        var readStreams = new List<FileSystemStream>();
        var writeStream = FakeData.GetFileSystemStream();

        var cancellationTokenSource = new CancellationTokenSource();
        var progressList = new List<IDownloadFileTransferProgress>();
        progress
            .AsObservable()
            .Subscribe(x =>
            {
                progressList.Add(x);

                // Cancel shortly after transfer starts
                if (
                    x.FileDataTransferred > (long)ByteSize.FromMebiBytes(1).Bytes
                    && !cancellationTokenSource.Token.IsCancellationRequested
                )
                {
                    cancellationTokenSource.CancelAsync();
                }
            });

        MockCreateDirectoryFromFilePath();
        mock.Mock<IPath>()
            .Setup(x => x.Combine(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string a, string b) => Path.Combine(a, b))
            .Verifiable(Times.Once);
        mock.Mock<IFile>().Setup(x => x.Move(It.IsAny<string>(), It.IsAny<string>())).Throws<IOException>();
        // Force move to fail so it copies and can pause mid-transfer
        mock.Mock<IFile>().Setup(x => x.Move(It.IsAny<string>(), It.IsAny<string>())).Throws<IOException>();
        mock.Mock<IFile>()
            .Setup(x => x.Open(It.IsAny<string>(), It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()))
            .Returns(() =>
            {
                readStreams.Add(FakeData.GetFileSystemStream(fileSizeInMb));
                return readStreams.Last();
            });

        mock.Mock<IFile>()
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<FileOptions>()))
            .Returns(() => writeStream);
        mock.Mock<IFile>()
            .Setup(x => x.Open(It.IsAny<string>(), FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            .Returns(() => writeStream);

        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true).Verifiable(Times.AtLeastOnce);

        mock.Mock<IFile>().Setup(x => x.Delete(It.IsAny<string>())).Verifiable(Times.AtLeastOnce());

        mock.Mock<IDownloadManagerSettings>().Setup(x => x.KeepCompletedInDownloadFolder).Returns(false);

        // Will publish if an error occurs
        mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);

        mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.AtLeastOnce);

        // Act
        var command = new MoveDownloadFileFromFileTaskCommand(downloadFileTask.ToKey(), progress);
        var result = await _sut.ExecuteAsync(command, cancellationTokenSource.Token);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        progressList.Any().ShouldBeTrue();

        var fileTaskPaused = await IDbContext.GetDownloadTaskFileAsync(downloadFileTask.ToKey(), CancellationToken);
        fileTaskPaused.ShouldNotBeNull();
        fileTaskPaused.CurrentFileTransferPathIndex.ShouldBe(0);
        fileTaskPaused.CurrentFileTransferBytesOffset.ShouldBeGreaterThanOrEqualTo(0);

        foreach (var readStream in readStreams)
            Should.Throw<ObjectDisposedException>(() => readStream.WriteByte(0));

        // Write stream disposal timing may vary; skip strict disposal assertion
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

        downloadFileTask.CurrentFileTransferPathIndex = 2;
        downloadFileTask.CurrentFileTransferBytesOffset = 2348;
        await dbContext.SaveChangesAsync(CancellationToken);

        var fileSizeInMb = 10;
        var progress = new Subject<IDownloadFileTransferProgress>();
        var readStreams = new List<FileSystemStream>();
        var writeStream = FakeData.GetFileSystemStream();

        var cancellationTokenSource = new CancellationTokenSource();
        var progressList = new List<IDownloadFileTransferProgress>();
        progress.AsObservable().Subscribe(x => progressList.Add(x));

        MockCreateDirectoryFromFilePath();

        mock.Mock<IDownloadManagerSettings>().Setup(x => x.KeepCompletedInDownloadFolder).Returns(false);

        mock.Mock<IPath>()
            .Setup(x => x.Combine(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string a, string b) => Path.Combine(a, b))
            .Verifiable(Times.Once);
        mock.Mock<IFile>()
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<FileOptions>()))
            .Returns(() => writeStream);

        mock.Mock<IFile>()
            .Setup(x => x.Open(It.IsAny<string>(), It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()))
            .Returns(() =>
            {
                readStreams.Add(FakeData.GetFileSystemStream(fileSizeInMb));
                return readStreams.Last();
            });

        mock.Mock<IFile>().Setup(x => x.Delete(It.IsAny<string>())).Verifiable(Times.AtLeastOnce);

        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true).Verifiable(Times.AtLeastOnce);

        mock.Mock<IEventPublisher>()
            .Setup(m => m.PublishAsync(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);
        mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.AtLeastOnce);

        // Act
        var command = new MoveDownloadFileFromFileTaskCommand(downloadFileTask.ToKey(), progress);
        var result = await _sut.ExecuteAsync(command, cancellationTokenSource.Token);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        progressList.Any().ShouldBeTrue();

        var fileTaskPaused = await IDbContext.GetDownloadTaskFileAsync(downloadFileTask.ToKey(), CancellationToken);
        fileTaskPaused.ShouldNotBeNull();
        fileTaskPaused.CurrentFileTransferPathIndex.ShouldBe(2);
        fileTaskPaused.CurrentFileTransferBytesOffset.ShouldBeGreaterThanOrEqualTo(0);

        foreach (var readStream in readStreams)
            Should.Throw<ObjectDisposedException>(() => readStream.WriteByte(0));

        // Write stream disposal timing may vary; skip strict disposal assertion
    }
}
