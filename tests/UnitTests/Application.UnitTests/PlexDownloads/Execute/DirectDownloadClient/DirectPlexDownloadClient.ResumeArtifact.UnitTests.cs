using System.ComponentModel;
using Downloader;
using DownloadStatus = Reaparr.Domain.DownloadStatus;

namespace Reaparr.Application.UnitTests;

public class DirectPlexDownloadClientResumeArtifactUnitTests : BaseUnitTest<DirectPlexDownloadClient>
{
    [Test]
    [Arguments(-1)]
    [Arguments(19)]
    public async Task ShouldReturnStorageErrorAndPreserveSnapshot_WhenPartialFileCannotContainSavedRanges(int partialLength)
    {
        // Arrange
        var (task, snapshot, fileSystem) = await CreateScenario(partialLength);
        var key = task.ToKey();
        Mock.Mock<IDownloadManagerSettings>().Setup(x => x.DownloadSegments).Returns(1);
        var partialPath = snapshot.FileName + snapshot.DownloadingFileExtension;
        var downloader = new Mock<IDownloadService>(MockBehavior.Strict);
        downloader.As<IAsyncDisposable>().Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.Is<GetDirectDownloadUrlCommand>(c => c.PlexServerId == key.PlexServerId), CancellationToken))
            .ReturnsAsync(Result.Ok("http://plex/resume.mkv")).Verifiable(Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.Is<EnsureDownloadDirectoryCommand>(c => c.Directory == task.DownloadDirectory && c.FileSize == task.DataTotal), CancellationToken))
            .ReturnsAsync(Result.Ok()).Verifiable(Times.Once());
        Mock.Mock<IDownloadSpeedLimitProvider>()
            .Setup(x => x.GetEffectiveDownloadSpeedLimitObservable(It.IsAny<string>()))
            .Returns(Observable.Return(0L)).Verifiable(Times.Once());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x => x.OnStatusChangedAsync(key, DownloadStatus.Downloading, CancellationToken.None))
            .Returns(Task.CompletedTask).Verifiable(Times.Once());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x => x.OnStatusChangedAsync(key, DownloadStatus.StorageError, It.Is<Result>(r => r.IsFailed && r.Errors.Count == 1), CancellationToken.None))
            .Returns(Task.CompletedTask).Verifiable(Times.Once());
        await using var sut = Mock.Create<DirectPlexDownloadClient>(
            new NamedParameter("downloadServiceFactory", (Func<DownloadConfiguration, IDownloadService>)(_ => downloader.Object)));

        // Act
        var result = await sut.Start(key, CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);
        result.HasStorageError().ShouldBeTrue();
        var persisted = await IDbContext.GetDownloadTaskFileAsync(key, CancellationToken);
        persisted.ShouldNotBeNull();
        persisted.DirectDownloadSnapshot.ShouldNotBeNull();
        persisted.DirectDownloadSnapshot.FileName.ShouldBe(snapshot.FileName);
        persisted.DirectDownloadSnapshot.Chunks.Select(x => (x.Start, x.End, x.Position))
            .ShouldBe(snapshot.Chunks.Select(x => (x.Start, x.End, x.Position)));
        fileSystem.File.Exists(partialPath).ShouldBe(partialLength >= 0);
        if (partialLength >= 0)
            fileSystem.File.ReadAllBytes(partialPath).ShouldBe(Enumerable.Repeat((byte)7, partialLength).ToArray());
        fileSystem.File.Exists(snapshot.FileName).ShouldBeFalse();
        downloader.Verify(x => x.DownloadFileTaskAsync(It.IsAny<DownloadPackage>(), It.IsAny<CancellationToken>()), Times.Never());
        downloader.Verify(x => x.DownloadFileTaskAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
        Mock.Mock<IDownloadTaskUpdateDispatcher>().Verify(x => x.OnStatusChangedAsync(key, DownloadStatus.DownloadFinished, It.IsAny<CancellationToken>()), Times.Never());
        Mock.Mock<ICommandExecutor>().Verify();
        Mock.Mock<IDownloadSpeedLimitProvider>().Verify();
        Mock.Mock<IDownloadTaskUpdateDispatcher>().Verify();
    }

    [Test]
    public async Task ShouldFinishResumedDownload_WhenPartialFileContainsAllSavedRanges()
    {
        // Arrange
        var (task, snapshot, fileSystem) = await CreateScenario(20);
        Mock.Mock<IDownloadManagerSettings>().Setup(x => x.DownloadSegments).Returns(1);
        var key = task.ToKey();
        var downloader = new Mock<IDownloadService>(MockBehavior.Strict);
        downloader.As<IAsyncDisposable>().Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);
        var output = Enumerable.Range(1, 32).Select(x => (byte)x).ToArray();
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.Is<GetDirectDownloadUrlCommand>(c => c.PlexServerId == key.PlexServerId), CancellationToken))
            .ReturnsAsync(Result.Ok("http://plex/resume.mkv")).Verifiable(Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.Is<EnsureDownloadDirectoryCommand>(c => c.Directory == task.DownloadDirectory && c.FileSize == task.DataTotal), CancellationToken))
            .ReturnsAsync(Result.Ok()).Verifiable(Times.Once());
        Mock.Mock<IDownloadSpeedLimitProvider>()
            .Setup(x => x.GetEffectiveDownloadSpeedLimitObservable(It.IsAny<string>()))
            .Returns(Observable.Return(0L)).Verifiable(Times.Once());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x => x.OnStatusChangedAsync(key, DownloadStatus.Downloading, CancellationToken.None))
            .Returns(Task.CompletedTask).Verifiable(Times.Once());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x => x.OnStatusChangedAsync(key, DownloadStatus.DownloadFinished, CancellationToken.None))
            .Returns(Task.CompletedTask).Verifiable(Times.Once());
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x => x.OnProgressUpdated(key, It.Is<DownloadTaskProgress>(p => p.DataTotal == 32 && p.DataReceived == 32 && p.Percentage == 100), It.IsAny<DirectDownloadSnapshot>()))
            .Verifiable(Times.Once());
        downloader.Setup(x => x.DownloadFileTaskAsync(It.Is<DownloadPackage>(p => p.Chunks[0].Position == 8 && p.Chunks[1].Position == 4 && p.Urls.Single() == "http://plex/resume.mkv"), CancellationToken))
            .Returns<DownloadPackage, CancellationToken>((package, _) =>
            {
                fileSystem.File.WriteAllBytes(task.DownloadFilePath, output);
                downloader.Raise(x => x.DownloadFileCompleted += null, downloader.Object, new AsyncCompletedEventArgs(null, false, package));
                return Task.FromResult(Stream.Null);
            }).Verifiable(Times.Once());
        await using var sut = Mock.Create<DirectPlexDownloadClient>(
            new NamedParameter("downloadServiceFactory", (Func<DownloadConfiguration, IDownloadService>)(_ => downloader.Object)));

        // Act
        var result = await sut.Start(key, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        fileSystem.File.ReadAllBytes(task.DownloadFilePath).ShouldBe(output);
        Mock.Mock<IDownloadTaskUpdateDispatcher>().Verify(x => x.OnStatusChangedAsync(key, DownloadStatus.StorageError, It.IsAny<Result>(), It.IsAny<CancellationToken>()), Times.Never());
        downloader.Verify(x => x.DownloadFileTaskAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
        downloader.Verify();
        Mock.Mock<ICommandExecutor>().Verify();
        Mock.Mock<IDownloadSpeedLimitProvider>().Verify();
        Mock.Mock<IDownloadTaskUpdateDispatcher>().Verify();
    }

    private async Task<(DownloadTaskMovieFile Task, DirectDownloadSnapshot Snapshot, IFileSystem FileSystem)> CreateScenario(int partialLength)
    {
        await SetupDatabase(51001, config =>
        {
            config.PlexServerCount = 1;
            config.PlexAccountCount = 1;
            config.MovieDownloadTasksCount = 1;
        });
        SetupFileSystem();
        SetupDependencies(builder => builder.Register(ctx => ctx.Resolve<IFileSystem>().FileInfo).As<IFileInfoFactory>());
        var fileSystem = Mock.Container.Resolve<IFileSystem>();
        var dbContext = IDbContext;
        var task = await dbContext.DownloadTaskMovieFile.SingleAsync(CancellationToken);
        var snapshot = new DirectDownloadSnapshot
        {
            SaveProgress = 37.5,
            Status = (int)Downloader.DownloadStatus.Stopped,
            Urls = ["http://old-plex/resume.mkv"],
            TotalFileSize = 32,
            FileName = Path.Combine(task.DownloadDirectory, task.FileName),
            DownloadingFileExtension = FilePathExtensions.TempDownloadFileSuffix,
            IsSupportDownloadInRange = true,
            Chunks =
            [
                new() { Id = "first", Start = 0, End = 15, Position = 8, MaxTryAgainOnFailure = 3, Timeout = 1000 },
                new() { Id = "second", Start = 16, End = 31, Position = 4, MaxTryAgainOnFailure = 3, Timeout = 1000 },
            ],
        };
        await dbContext.DownloadTaskMovieFile.Where(x => x.Id == task.Id)
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.DirectDownloadSnapshot, snapshot).SetProperty(p => p.DataTotal, 32), CancellationToken);
        task = await dbContext.DownloadTaskMovieFile.SingleAsync(CancellationToken);
        fileSystem.Directory.CreateDirectory(task.DownloadDirectory);
        if (partialLength >= 0)
            fileSystem.File.WriteAllBytes(snapshot.FileName + snapshot.DownloadingFileExtension, Enumerable.Repeat((byte)7, partialLength).ToArray());
        return (task, snapshot, fileSystem);
    }
}
