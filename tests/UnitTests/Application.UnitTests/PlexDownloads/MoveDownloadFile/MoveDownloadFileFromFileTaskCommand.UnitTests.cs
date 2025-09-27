using System.IO.Abstractions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ByteSizeLib;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

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

        mock.Mock<IFile>()
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<FileOptions>()))
            .Throws<UnauthorizedAccessException>();

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
        downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.MergeError);
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

        mock.Mock<IDirectory>()
            .Setup(x => x.CreateDirectory(It.IsAny<string>()))
            .Returns(new Mock<IDirectoryInfo>().Object)
            .Verifiable(Times.Once);

        mock.Mock<IPath>().Setup(x => x.GetDirectoryName(It.IsAny<string>())).Returns("folder").Verifiable(Times.Once);
        mock.Mock<IFile>()
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<FileOptions>()))
            .Returns(() => writeStream);
        mock.Mock<IFile>()
            .Setup(x => x.Open(It.IsAny<string>(), It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()))
            .Returns(() =>
            {
                readStreams.Add(FakeData.GetFileSystemStream(fileSizeInMb));
                return readStreams.Last();
            })
            .Verifiable(Times.Exactly(1));
        mock.Mock<IFile>().Setup(x => x.Delete(It.IsAny<string>())).Verifiable(Times.Exactly(1));
        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true).Verifiable(Times.Exactly(4));

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
        fileTaskPaused.CurrentFileTransferPathIndex.ShouldBe(3);
        fileTaskPaused.CurrentFileTransferBytesOffset.ShouldBe(0);

        foreach (var readStream in readStreams)
            Should.Throw<ObjectDisposedException>(() => readStream.WriteByte(0));

        Should.Throw<ObjectDisposedException>(() => writeStream.WriteByte(0));
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

                // Wait until 1 file has been merged
                if (
                    x.FileDataTransferred > (long)ByteSize.FromMebiBytes(fileSizeInMb + 2).Bytes
                    && !cancellationTokenSource.Token.IsCancellationRequested
                )
                {
                    cancellationTokenSource.CancelAsync();
                }
            });

        MockCreateDirectoryFromFilePath();

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

        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true).Verifiable(Times.AtLeastOnce);

        mock.Mock<IFile>().Setup(x => x.Delete(It.IsAny<string>())).Verifiable(Times.AtLeastOnce());

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
        fileTaskPaused.CurrentFileTransferPathIndex.ShouldBe(1);
        fileTaskPaused.CurrentFileTransferBytesOffset.ShouldBeGreaterThan(0);

        foreach (var readStream in readStreams)
            Should.Throw<ObjectDisposedException>(() => readStream.WriteByte(0));

        Should.Throw<ObjectDisposedException>(() => writeStream.WriteByte(0));
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

        mock.Mock<IFile>().Setup(x => x.Delete(It.IsAny<string>())).Verifiable(Times.Exactly(2));

        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true).Verifiable(Times.Exactly(2));

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
        fileTaskPaused.CurrentFileTransferPathIndex.ShouldBe(3);
        fileTaskPaused.CurrentFileTransferBytesOffset.ShouldBe(0);

        foreach (var readStream in readStreams)
            Should.Throw<ObjectDisposedException>(() => readStream.WriteByte(0));

        Should.Throw<ObjectDisposedException>(() => writeStream.WriteByte(0));
    }

    [Fact]
    public async Task ShouldMergedAllDataCorrectly_WhenCompleted()
    {
        // Arrange
        var fileSizeInMb = 10;
        var fileParts = 4;
        await SetupDatabase(
            52223,
            config =>
            {
                config.DownloadFileSizeInMb = fileSizeInMb;
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
                config.DownloadWorkerTasks = fileParts;
            }
        );

        var dbContext = IDbContext;
        var downloadFileTask = await dbContext
            .DownloadTaskTvShowEpisodeFile.AsTracking()
            .Include(x => x.DownloadWorkerTasks)
            .FirstOrDefaultAsync(CancellationToken);
        downloadFileTask.ShouldNotBeNull();

        var progress = new Subject<IDownloadFileTransferProgress>();
        var readStreams = new List<FileSystemStream>();
        var writeStream = FakeData.GetFileSystemStream();
        var cancellationTokenSource = new CancellationTokenSource();
        var progressList = new List<IDownloadFileTransferProgress>();
        progress.AsObservable().Subscribe(x => progressList.Add(x));

        MockCreateDirectoryFromFilePath();

        mock.Mock<IFile>()
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<FileOptions>()))
            .Returns(() => writeStream);

        mock.Mock<IFile>()
            .Setup(x => x.Open(It.IsAny<string>(), It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()))
            .Returns(() =>
            {
                readStreams.Add(
                    FakeData.GetFileSystemStream(decimal.ToDouble(decimal.Divide(fileSizeInMb, fileParts)))
                );
                return readStreams.Last();
            });

        mock.Mock<IFile>().Setup(x => x.Delete(It.IsAny<string>())).Verifiable(Times.Exactly(4));
        mock.Mock<IFile>().Setup(x => x.Exists(It.IsAny<string>())).Returns(true).Verifiable((Times.Exactly(4)));

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

        var downloadTaskCompleted = await IDbContext.GetDownloadTaskFileAsync(
            downloadFileTask.ToKey(),
            CancellationToken
        );
        downloadTaskCompleted.ShouldNotBeNull();
        downloadTaskCompleted.CurrentFileTransferPathIndex.ShouldBe(3);
        downloadTaskCompleted.CurrentFileTransferBytesOffset.ShouldBe(0);

        downloadTaskCompleted.DownloadStatus.ShouldBe(DownloadStatus.MergeFinished);
        downloadTaskCompleted.FileDataTransferred.ShouldBe(downloadTaskCompleted.DataTotal);

        foreach (var readStream in readStreams)
            Should.Throw<ObjectDisposedException>(() => readStream.WriteByte(0));

        Should.Throw<ObjectDisposedException>(() => writeStream.WriteByte(0));
    }
}
