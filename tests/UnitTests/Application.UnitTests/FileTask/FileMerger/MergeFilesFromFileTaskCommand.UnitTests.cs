using System.Reactive.Linq;
using System.Reactive.Subjects;
using Application.Contracts;
using ByteSizeLib;
using Data.Contracts;
using FileSystem.Contracts;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application.UnitTests;

public class MergeFilesFromFileTaskCommandUnitTests : BaseUnitTest<MergeFilesFromFileTaskCommandHandler>
{
    public MergeFilesFromFileTaskCommandUnitTests(ITestOutputHelper output)
        : base(output) { }

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

        var key = await IDbContext.DownloadTaskTvShowEpisodeFile.Select(x => x.ToKey()).FirstOrDefaultAsync();
        key.ShouldNotBeNull();

        var progress = new Subject<IDownloadFileTransferProgress>();

        mock.Mock<IDirectorySystem>()
            .Setup(x => x.CreateDirectoryFromFilePath(It.IsAny<string>()))
            .Returns(Result.Fail("Failed to create directory"))
            .Verifiable(Times.Once);
        mock.Mock<IMediator>()
            .Setup(m => m.Publish(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);
        mock.SetupMediator(It.IsAny<DownloadTaskUpdatedNotification>)
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);

        // Act
        var command = new MergeFilesFromFileTaskCommand(key, progress);
        var result = await _sut.Handle(command, CancellationToken.None);

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

        var key = await IDbContext.DownloadTaskTvShowEpisodeFile.Select(x => x.ToKey()).FirstOrDefaultAsync();
        key.ShouldNotBeNull();

        var progress = new Subject<IDownloadFileTransferProgress>();

        mock.Mock<IDirectorySystem>()
            .Setup(x => x.CreateDirectoryFromFilePath(It.IsAny<string>()))
            .Returns(Result.Fail(new Error("")))
            .Verifiable(Times.Once);
        mock.Mock<IMediator>()
            .Setup(m => m.Publish(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);
        mock.SetupMediator(It.IsAny<DownloadTaskUpdatedNotification>)
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);

        // Act
        var command = new MergeFilesFromFileTaskCommand(key, progress);
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();

        var downloadTaskDb = await IDbContext.GetDownloadTaskFileAsync(key);
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
            .FirstOrDefaultAsync();
        downloadFileTask.ShouldNotBeNull();

        var fileSizeInMb = 10;
        var progress = new Subject<IDownloadFileTransferProgress>();
        var readStreams = new List<MemoryStream>();
        var writeStream = new MemoryStream();

        var progressList = new List<IDownloadFileTransferProgress>();
        progress.AsObservable().Subscribe(x => progressList.Add(x));

        mock.Mock<IFileResultSystem>()
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<FileOptions>()))
            .Returns(() => Result.Ok<Stream>(writeStream));
        mock.Mock<IFileResultSystem>()
            .Setup(x => x.Open(It.IsAny<string>(), It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()))
            .Returns(() =>
            {
                readStreams.Add(FakeData.GetFileStream(fileSizeInMb));
                return Result.Ok<Stream>(readStreams.Last());
            })
            .Verifiable(Times.Exactly(downloadFileTask.FilePaths.Count));
        mock.Mock<IFileResultSystem>()
            .Setup(x => x.DeleteFile(It.IsAny<string>()))
            .Returns(Result.Ok())
            .Verifiable(Times.Exactly(downloadFileTask.FilePaths.Count));
        mock.Mock<IDirectorySystem>()
            .Setup(x => x.CreateDirectoryFromFilePath(It.IsAny<string>()))
            .Returns(Result.Ok())
            .Verifiable(Times.Exactly(1));
        mock.Mock<IFileResultSystem>()
            .Setup(x => x.FileExists(It.IsAny<string>()))
            .Returns(true)
            .Verifiable(Times.Exactly(4));
        mock.Mock<IMediator>()
            .Setup(m => m.Publish(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);

        mock.SetupMediator(It.IsAny<DownloadTaskUpdatedNotification>)
            .Returns(Task.CompletedTask)
            .Verifiable(Times.AtLeastOnce);

        // Act
        var command = new MergeFilesFromFileTaskCommand(downloadFileTask.ToKey(), progress);
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        progressList.Any().ShouldBeTrue();

        var fileTaskPaused = await IDbContext.GetDownloadTaskFileAsync(downloadFileTask.ToKey());
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
            .FirstOrDefaultAsync();
        downloadFileTask.ShouldNotBeNull();

        var fileSizeInMb = 10;
        var progress = new Subject<IDownloadFileTransferProgress>();
        var readStreams = new List<MemoryStream>();
        var writeStream = new MemoryStream();

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

        mock.Mock<IFileResultSystem>()
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<FileOptions>()))
            .Returns(() => Result.Ok<Stream>(writeStream));
        mock.Mock<IFileResultSystem>()
            .Setup(x => x.Open(It.IsAny<string>(), It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()))
            .Returns(() =>
            {
                readStreams.Add(FakeData.GetFileStream(fileSizeInMb));
                return Result.Ok<Stream>(readStreams.Last());
            });
        mock.Mock<IFileResultSystem>()
            .Setup(x => x.DeleteFile(It.IsAny<string>()))
            .Returns(Result.Ok())
            .Verifiable(Times.AtLeastOnce());
        mock.Mock<IDirectorySystem>()
            .Setup(x => x.CreateDirectoryFromFilePath(It.IsAny<string>()))
            .Returns(Result.Ok())
            .Verifiable(Times.Exactly(1));
        mock.Mock<IMediator>()
            .Setup(m => m.Publish(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);
        mock.SetupMediator(It.IsAny<DownloadTaskUpdatedNotification>)
            .Returns(Task.CompletedTask)
            .Verifiable(Times.AtLeastOnce);
        mock.Mock<IFileResultSystem>()
            .Setup(x => x.FileExists(It.IsAny<string>()))
            .Returns(true)
            .Verifiable(Times.AtLeastOnce);

        // Act
        var command = new MergeFilesFromFileTaskCommand(downloadFileTask.ToKey(), progress);
        var result = await _sut.Handle(command, cancellationTokenSource.Token);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        progressList.Any().ShouldBeTrue();

        var fileTaskPaused = await IDbContext.GetDownloadTaskFileAsync(
            downloadFileTask.ToKey(),
            CancellationToken.None
        );
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
            .FirstOrDefaultAsync();
        downloadFileTask.ShouldNotBeNull();

        downloadFileTask.CurrentFileTransferPathIndex = 2;
        downloadFileTask.CurrentFileTransferBytesOffset = 2348;
        await dbContext.SaveChangesAsync();

        var fileSizeInMb = 10;
        var progress = new Subject<IDownloadFileTransferProgress>();
        var readStreams = new List<MemoryStream>();
        var writeStream = new MemoryStream();

        var cancellationTokenSource = new CancellationTokenSource();
        var progressList = new List<IDownloadFileTransferProgress>();
        progress.AsObservable().Subscribe(x => progressList.Add(x));

        mock.Mock<IFileResultSystem>()
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<FileOptions>()))
            .Returns(() => Result.Ok<Stream>(writeStream));
        mock.Mock<IFileResultSystem>()
            .Setup(x => x.Open(It.IsAny<string>(), It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()))
            .Returns(() =>
            {
                readStreams.Add(FakeData.GetFileStream(fileSizeInMb));
                return Result.Ok<Stream>(readStreams.Last());
            });
        mock.Mock<IFileResultSystem>()
            .Setup(x => x.DeleteFile(It.IsAny<string>()))
            .Returns(Result.Ok())
            .Verifiable(Times.Exactly(2));
        mock.Mock<IDirectorySystem>()
            .Setup(x => x.CreateDirectoryFromFilePath(It.IsAny<string>()))
            .Returns(Result.Ok())
            .Verifiable(Times.Exactly(1));
        mock.Mock<IMediator>()
            .Setup(m => m.Publish(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);
        mock.SetupMediator(It.IsAny<DownloadTaskUpdatedNotification>)
            .Returns(Task.CompletedTask)
            .Verifiable(Times.AtLeastOnce);
        mock.Mock<IFileResultSystem>()
            .Setup(x => x.FileExists(It.IsAny<string>()))
            .Returns(true)
            .Verifiable(Times.Exactly(2));

        // Act
        var command = new MergeFilesFromFileTaskCommand(downloadFileTask.ToKey(), progress);
        var result = await _sut.Handle(command, cancellationTokenSource.Token);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        progressList.Any().ShouldBeTrue();

        var fileTaskPaused = await IDbContext.GetDownloadTaskFileAsync(
            downloadFileTask.ToKey(),
            CancellationToken.None
        );
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
            .FirstOrDefaultAsync();
        downloadFileTask.ShouldNotBeNull();

        var progress = new Subject<IDownloadFileTransferProgress>();
        var readStreams = new List<MemoryStream>();
        var writeStream = new MemoryStream();
        var cancellationTokenSource = new CancellationTokenSource();
        var progressList = new List<IDownloadFileTransferProgress>();
        progress.AsObservable().Subscribe(x => progressList.Add(x));

        mock.Mock<IFileResultSystem>()
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<FileOptions>()))
            .Returns(() => Result.Ok<Stream>(writeStream));
        mock.Mock<IFileResultSystem>()
            .Setup(x => x.Open(It.IsAny<string>(), It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()))
            .Returns(() =>
            {
                readStreams.Add(FakeData.GetFileStream(decimal.ToDouble(decimal.Divide(fileSizeInMb, fileParts))));
                return Result.Ok<Stream>(readStreams.Last());
            });
        mock.Mock<IFileResultSystem>()
            .Setup(x => x.DeleteFile(It.IsAny<string>()))
            .Returns(Result.Ok())
            .Verifiable(Times.Exactly(4));
        mock.Mock<IDirectorySystem>()
            .Setup(x => x.CreateDirectoryFromFilePath(It.IsAny<string>()))
            .Returns(Result.Ok())
            .Verifiable(Times.Exactly(1));
        mock.Mock<IMediator>()
            .Setup(m => m.Publish(It.IsAny<SendNotificationResult>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Never);
        mock.SetupMediator(It.IsAny<DownloadTaskUpdatedNotification>)
            .Returns(Task.CompletedTask)
            .Verifiable(Times.AtLeastOnce);
        mock.Mock<IFileResultSystem>()
            .Setup(x => x.FileExists(It.IsAny<string>()))
            .Returns(true)
            .Verifiable(Times.Exactly(4));

        // Act
        var command = new MergeFilesFromFileTaskCommand(downloadFileTask.ToKey(), progress);
        var result = await _sut.Handle(command, cancellationTokenSource.Token);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        progressList.Any().ShouldBeTrue();

        var downloadTaskCompleted = await IDbContext.GetDownloadTaskFileAsync(
            downloadFileTask.ToKey(),
            CancellationToken.None
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
