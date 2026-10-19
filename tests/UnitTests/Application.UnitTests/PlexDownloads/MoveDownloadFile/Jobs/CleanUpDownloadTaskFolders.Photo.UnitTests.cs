namespace Reaparr.Application.UnitTests;

public class CleanUpDownloadTaskFoldersPhotoUnitTests : BaseCommandUnitTest<CleanUpDownloadTaskFoldersCommand>
{
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task ShouldRemoveOnlyEmptyMediaFolder_WhenCleaningCompletedPhoto(bool containsFile)
    {
        // Arrange
        await SetupDatabase(64001, config => config.PlexPhotoLibraryCount = 1);
        var dbContext = IDbContext;
        var library = await dbContext.PlexLibraries.SingleAsync(x => x.Type == PlexMediaType.PhotoAlbum);
        var album = FakeData
            .GetDownloadTaskPhotoFolder(new Seed(64001))
            .RuleFor(x => x.Files, _ => FakeData.GetDownloadTaskPhotoImageFile(new Seed(64002)).Generate(1))
            .Generate();
        var file = album.Files.Single();
        foreach (var node in new DownloadTaskBase[] { album }.Concat(album.Files))
        {
            node.PlexServerId = library.PlexServerId;
            node.PlexLibraryId = library.Id;
        }
        dbContext.DownloadTaskPhotoFolders.Add(album);
        var paths = Mock.Container.Resolve<IPathProvider>();
        file.DirectoryMeta.DownloadRootPath = paths.DefaultDownloadsDestinationFolder;
        file.DirectoryMeta.PhotoFolder = "shared/file";
        file.DownloadStatus = DownloadStatus.Completed;
        await dbContext.SaveChangesAsync(CancellationToken);
        SetupFileSystem(fs =>
        {
            fs.AddDirectory(file.DownloadDirectory);
            if (containsFile)
                fs.AddFile(file.DownloadFilePath, new MockFileData("target bytes"));
        });

        // Act
        var result = await TestHandlerExecuteAsync(new CleanUpDownloadTaskFoldersCommand(file.ToKey()));

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        var fs = Mock.Container.Resolve<IFileSystem>();
        fs.Directory.Exists(file.DownloadDirectory).ShouldBe(containsFile);
        fs.Directory.Exists(Path.GetDirectoryName(file.DownloadDirectory)!).ShouldBe(containsFile);
        fs.Directory.Exists(file.DirectoryMeta.GetDownloadCategoryDirectory(file.DownloadTaskType)).ShouldBeTrue();
        if (containsFile)
            fs.File.ReadAllText(file.DownloadFilePath).ShouldBe("target bytes");
        (await dbContext.DownloadTaskPhotoImageFiles.Select(x => x.Id).SingleAsync(CancellationToken)).ShouldBe(file.Id);
    }

    [Test]
    [Arguments(DownloadStatus.Downloading)]
    [Arguments(DownloadStatus.Completed)]
    [Arguments(DownloadStatus.Deleted)]
    public async Task ShouldRespectMovieSiblingStatusWhenCleaningPhoto(DownloadStatus siblingStatus)
    {
        // Arrange
        await SetupDatabase(64002, config =>
        {
            config.MovieDownloadTasksCount = 1;
            config.PlexPhotoLibraryCount = 1;
        });
        var dbContext = IDbContext;
        var library = await dbContext.PlexLibraries.SingleAsync(x => x.Type == PlexMediaType.PhotoAlbum);
        var album = FakeData
            .GetDownloadTaskPhotoFolder(new Seed(64002))
            .RuleFor(x => x.Files, _ => FakeData.GetDownloadTaskPhotoImageFile(new Seed(64003)).Generate(3))
            .Generate();
        var photoFiles = album.Files.Cast<DownloadTaskFileBase>().ToList();
        foreach (var node in new DownloadTaskBase[] { album }.Concat(photoFiles))
        {
            node.PlexServerId = library.PlexServerId;
            node.PlexLibraryId = library.Id;
        }
        dbContext.DownloadTaskPhotoFolders.Add(album);
        var target = photoFiles[0];
        var control = photoFiles[2];
        var sibling = await dbContext.DownloadTaskMovieFile.AsTracking().SingleAsync(CancellationToken);
        var paths = Mock.Container.Resolve<IPathProvider>();
        var root = paths.DefaultDownloadsDestinationFolder;
        target.DirectoryMeta.DownloadRootPath = root;
        target.DirectoryMeta.PhotoFolder = "Movies/shared/file";
        target.DownloadStatus = DownloadStatus.Completed;
        control.DirectoryMeta.DownloadRootPath = root;
        control.DirectoryMeta.PhotoFolder = "Photos/control";
        control.DownloadStatus = DownloadStatus.Downloading;
        sibling.DirectoryMeta.DownloadRootPath = Path.Combine(root, "Photos");
        sibling.DirectoryMeta.MovieFolder = "shared/file";
        sibling.DownloadStatus = siblingStatus;
        dbContext.Entry(sibling).State = EntityState.Modified;
        await dbContext.SaveChangesAsync(CancellationToken);
        target.DownloadDirectory.ShouldBe(sibling.DownloadDirectory);
        target.DownloadDirectory.ShouldNotBe(control.DownloadDirectory);
        SetupFileSystem(fs => fs.AddDirectory(target.DownloadDirectory));

        // Act
        var result = await TestHandlerExecuteAsync(new CleanUpDownloadTaskFoldersCommand(target.ToKey()));

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        var fs = Mock.Container.Resolve<IFileSystem>();
        var active = siblingStatus is not (DownloadStatus.Completed or DownloadStatus.Deleted);
        fs.Directory.Exists(target.DownloadDirectory).ShouldBe(active);
        fs.Directory.Exists(Path.GetDirectoryName(target.DownloadDirectory)!).ShouldBe(active);
        (await dbContext.DownloadTaskPhotoImageFiles.Select(x => x.Id).OrderBy(x => x).ToListAsync(CancellationToken))
            .ShouldBe(photoFiles.Select(x => x.Id).Order());
        (await dbContext.DownloadTaskMovieFile.Select(x => x.Id).SingleAsync(CancellationToken)).ShouldBe(sibling.Id);
    }

    [Test]
    [Arguments(DownloadStatus.Downloading)]
    [Arguments(DownloadStatus.Completed)]
    [Arguments(DownloadStatus.Deleted)]
    public async Task ShouldRespectEpisodeSiblingStatusWhenCleaningPhoto(DownloadStatus siblingStatus)
    {
        // Arrange
        await SetupDatabase(64004, config =>
        {
            config.TvShowDownloadTasksCount = 1;
            config.TvShowSeasonDownloadTasksCount = 1;
            config.TvShowEpisodeDownloadTasksCount = 1;
            config.PlexPhotoLibraryCount = 1;
        });
        var dbContext = IDbContext;
        var library = await dbContext.PlexLibraries.SingleAsync(x => x.Type == PlexMediaType.PhotoAlbum);
        var album = FakeData
            .GetDownloadTaskPhotoFolder(new Seed(64004))
            .RuleFor(x => x.Files, _ => FakeData.GetDownloadTaskPhotoImageFile(new Seed(64005)).Generate(3))
            .Generate();
        var photoFiles = album.Files.Cast<DownloadTaskFileBase>().ToList();
        foreach (var node in new DownloadTaskBase[] { album }.Concat(photoFiles))
        {
            node.PlexServerId = library.PlexServerId;
            node.PlexLibraryId = library.Id;
        }
        dbContext.DownloadTaskPhotoFolders.Add(album);
        var target = photoFiles[0];
        var control = photoFiles[2];
        var sibling = await dbContext.DownloadTaskTvShowEpisodeFile.AsTracking().SingleAsync(CancellationToken);
        var paths = Mock.Container.Resolve<IPathProvider>();
        var root = paths.DefaultDownloadsDestinationFolder;
        target.DirectoryMeta.DownloadRootPath = root;
        target.DirectoryMeta.PhotoFolder = "TvShows/shared/file";
        target.DownloadStatus = DownloadStatus.Completed;
        control.DirectoryMeta.DownloadRootPath = root;
        control.DirectoryMeta.PhotoFolder = "Photos/control";
        control.DownloadStatus = DownloadStatus.Downloading;
        sibling.DirectoryMeta.DownloadRootPath = Path.Combine(root, "Photos");
        sibling.DirectoryMeta.TvShowFolder = "shared";
        sibling.DirectoryMeta.SeasonFolder = "file";
        sibling.DownloadStatus = siblingStatus;
        dbContext.Entry(sibling).State = EntityState.Modified;
        await dbContext.SaveChangesAsync(CancellationToken);
        target.DownloadDirectory.ShouldBe(sibling.DownloadDirectory);
        target.DownloadDirectory.ShouldNotBe(control.DownloadDirectory);
        SetupFileSystem(fs => fs.AddDirectory(target.DownloadDirectory));

        // Act
        var result = await TestHandlerExecuteAsync(new CleanUpDownloadTaskFoldersCommand(target.ToKey()));

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        var fs = Mock.Container.Resolve<IFileSystem>();
        var active = siblingStatus is not (DownloadStatus.Completed or DownloadStatus.Deleted);
        fs.Directory.Exists(target.DownloadDirectory).ShouldBe(active);
        fs.Directory.Exists(Path.GetDirectoryName(target.DownloadDirectory)!).ShouldBe(active);
        (await dbContext.DownloadTaskPhotoImageFiles.Select(x => x.Id).OrderBy(x => x).ToListAsync(CancellationToken))
            .ShouldBe(photoFiles.Select(x => x.Id).Order());
        (await dbContext.DownloadTaskTvShowEpisodeFile.Select(x => x.Id).SingleAsync(CancellationToken)).ShouldBe(sibling.Id);
    }

    [Test]
    [Arguments(DownloadStatus.Downloading)]
    [Arguments(DownloadStatus.Completed)]
    [Arguments(DownloadStatus.Deleted)]
    public async Task ShouldRespectPhotoSiblingStatusWhenCleaningPhoto(DownloadStatus siblingStatus)
    {
        // Arrange
        await SetupDatabase(64005, config => config.PlexPhotoLibraryCount = 1);
        var dbContext = IDbContext;
        var library = await dbContext.PlexLibraries.SingleAsync(x => x.Type == PlexMediaType.PhotoAlbum);
        var album = FakeData
            .GetDownloadTaskPhotoFolder(new Seed(64005))
            .RuleFor(x => x.Files, _ => FakeData.GetDownloadTaskPhotoImageFile(new Seed(64006)).Generate(3))
            .Generate();
        var photoFiles = album.Files.Cast<DownloadTaskFileBase>().ToList();
        foreach (var node in new DownloadTaskBase[] { album }.Concat(photoFiles))
        {
            node.PlexServerId = library.PlexServerId;
            node.PlexLibraryId = library.Id;
        }
        dbContext.DownloadTaskPhotoFolders.Add(album);
        var target = photoFiles[0];
        var sibling = photoFiles[1];
        var control = photoFiles[2];
        var paths = Mock.Container.Resolve<IPathProvider>();
        var root = paths.DefaultDownloadsDestinationFolder;
        target.DirectoryMeta.DownloadRootPath = root;
        target.DirectoryMeta.PhotoFolder = "Photos/shared/file";
        target.DownloadStatus = DownloadStatus.Completed;
        sibling.DirectoryMeta.DownloadRootPath = Path.Combine(root, "Photos");
        sibling.DirectoryMeta.PhotoFolder = "shared/file";
        sibling.DownloadStatus = siblingStatus;
        control.DirectoryMeta.DownloadRootPath = root;
        control.DirectoryMeta.PhotoFolder = "Photos/control";
        control.DownloadStatus = DownloadStatus.Downloading;
        
        await dbContext.SaveChangesAsync(CancellationToken);
        target.DownloadDirectory.ShouldBe(sibling.DownloadDirectory);
        target.DownloadDirectory.ShouldNotBe(control.DownloadDirectory);
        SetupFileSystem(fs => fs.AddDirectory(target.DownloadDirectory));

        // Act
        var result = await TestHandlerExecuteAsync(new CleanUpDownloadTaskFoldersCommand(target.ToKey()));

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        var fs = Mock.Container.Resolve<IFileSystem>();
        var active = siblingStatus is not (DownloadStatus.Completed or DownloadStatus.Deleted);
        fs.Directory.Exists(target.DownloadDirectory).ShouldBe(active);
        fs.Directory.Exists(Path.GetDirectoryName(target.DownloadDirectory)!).ShouldBe(active);
        (await dbContext.DownloadTaskPhotoImageFiles.Select(x => x.Id).OrderBy(x => x).ToListAsync(CancellationToken))
            .ShouldBe(new[] { target.Id, sibling.Id, control.Id }.Order());
    }

    [Test]
    [Arguments(DownloadStatus.Downloading)]
    [Arguments(DownloadStatus.Completed)]
    [Arguments(DownloadStatus.Deleted)]
    public async Task ShouldRespectMusicSiblingStatusWhenCleaningPhoto(DownloadStatus siblingStatus)
    {
        // Arrange
        await SetupDatabase(64007, config =>
        {
            config.PlexPhotoLibraryCount = 1;
            config.PlexMusicLibraryCount = 1;
            config.MusicArtistDownloadTasksCount = 1;
            config.MusicAlbumDownloadTasksCount = 1;
            config.MusicTrackDownloadTasksCount = 1;
            config.MusicTrackFileDownloadTasksCount = 1;
        });
        var dbContext = IDbContext;
        var library = await dbContext.PlexLibraries.SingleAsync(x => x.Type == PlexMediaType.PhotoAlbum);
        var album = FakeData
            .GetDownloadTaskPhotoFolder(new Seed(64007))
            .RuleFor(x => x.Files, _ => FakeData.GetDownloadTaskPhotoImageFile(new Seed(64008)).Generate(3))
            .Generate();
        var photoFiles = album.Files.Cast<DownloadTaskFileBase>().ToList();
        foreach (var node in new DownloadTaskBase[] { album }.Concat(photoFiles))
        {
            node.PlexServerId = library.PlexServerId;
            node.PlexLibraryId = library.Id;
        }
        dbContext.DownloadTaskPhotoFolders.Add(album);
        var target = photoFiles[0];
        var control = photoFiles[2];
        var sibling = await dbContext.DownloadTaskMusicTrackFiles.SingleAsync(CancellationToken);
        var paths = Mock.Container.Resolve<IPathProvider>();
        var root = paths.DefaultDownloadsDestinationFolder;
        target.DirectoryMeta.DownloadRootPath = root;
        target.DirectoryMeta.PhotoFolder = "Music/shared/file";
        target.DownloadStatus = DownloadStatus.Completed;
        control.DirectoryMeta.DownloadRootPath = root;
        control.DirectoryMeta.PhotoFolder = "Photos/control";
        control.DownloadStatus = DownloadStatus.Downloading;
        sibling.DirectoryMeta.DownloadRootPath = Path.Combine(root, "Photos");
        sibling.DirectoryMeta.MusicArtistFolder = "shared";
        sibling.DirectoryMeta.MusicAlbumFolder = "file";
        sibling.DownloadStatus = siblingStatus;
        dbContext.Entry(sibling).State = EntityState.Modified;
        await dbContext.SaveChangesAsync(CancellationToken);
        target.DownloadDirectory.ShouldBe(sibling.DownloadDirectory);
        target.DownloadDirectory.ShouldNotBe(control.DownloadDirectory);
        SetupFileSystem(fs => fs.AddDirectory(target.DownloadDirectory));

        // Act
        var result = await TestHandlerExecuteAsync(new CleanUpDownloadTaskFoldersCommand(target.ToKey()));

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        var fs = Mock.Container.Resolve<IFileSystem>();
        var active = siblingStatus is not (DownloadStatus.Completed or DownloadStatus.Deleted);
        fs.Directory.Exists(target.DownloadDirectory).ShouldBe(active);
        fs.Directory.Exists(Path.GetDirectoryName(target.DownloadDirectory)!).ShouldBe(active);
        (await dbContext.DownloadTaskPhotoImageFiles.Select(x => x.Id).OrderBy(x => x).ToListAsync(CancellationToken))
            .ShouldBe(photoFiles.Select(x => x.Id).Order());
        (await dbContext.DownloadTaskMusicTrackFiles.Select(x => x.Id).SingleAsync(CancellationToken)).ShouldBe(sibling.Id);
    }

    [Test]
    [Arguments(DownloadStatus.Downloading)]
    [Arguments(DownloadStatus.Completed)]
    [Arguments(DownloadStatus.Deleted)]
    public async Task ShouldRespectOtherVideoSiblingStatusWhenCleaningPhoto(DownloadStatus siblingStatus)
    {
        // Arrange
        await SetupDatabase(64009, config =>
        {
            config.PlexPhotoLibraryCount = 1;
            config.PlexOtherVideoLibraryCount = 1;
            config.OtherVideoDownloadTasksCount = 1;
            config.OtherVideoFileDownloadTasksCount = 1;
        });
        var dbContext = IDbContext;
        var library = await dbContext.PlexLibraries.SingleAsync(x => x.Type == PlexMediaType.PhotoAlbum);
        var album = FakeData
            .GetDownloadTaskPhotoFolder(new Seed(64009))
            .RuleFor(x => x.Files, _ => FakeData.GetDownloadTaskPhotoImageFile(new Seed(64010)).Generate(3))
            .Generate();
        var photoFiles = album.Files.Cast<DownloadTaskFileBase>().ToList();
        foreach (var node in new DownloadTaskBase[] { album }.Concat(photoFiles))
        {
            node.PlexServerId = library.PlexServerId;
            node.PlexLibraryId = library.Id;
        }
        dbContext.DownloadTaskPhotoFolders.Add(album);
        var target = photoFiles[0];
        var control = photoFiles[2];
        var sibling = await dbContext.DownloadTaskOtherVideoFiles.SingleAsync(CancellationToken);
        var paths = Mock.Container.Resolve<IPathProvider>();
        var root = paths.DefaultDownloadsDestinationFolder;
        target.DirectoryMeta.DownloadRootPath = root;
        target.DirectoryMeta.PhotoFolder = "OtherVideos/shared/file";
        target.DownloadStatus = DownloadStatus.Completed;
        control.DirectoryMeta.DownloadRootPath = root;
        control.DirectoryMeta.PhotoFolder = "Photos/control";
        control.DownloadStatus = DownloadStatus.Downloading;
        sibling.DirectoryMeta.DownloadRootPath = Path.Combine(root, "Photos");
        sibling.DirectoryMeta.OtherVideoFolder = "shared/file";
        sibling.DownloadStatus = siblingStatus;
        dbContext.Entry(sibling).State = EntityState.Modified;
        await dbContext.SaveChangesAsync(CancellationToken);
        target.DownloadDirectory.ShouldBe(sibling.DownloadDirectory);
        target.DownloadDirectory.ShouldNotBe(control.DownloadDirectory);
        SetupFileSystem(fs => fs.AddDirectory(target.DownloadDirectory));

        // Act
        var result = await TestHandlerExecuteAsync(new CleanUpDownloadTaskFoldersCommand(target.ToKey()));

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        var fs = Mock.Container.Resolve<IFileSystem>();
        var active = siblingStatus is not (DownloadStatus.Completed or DownloadStatus.Deleted);
        fs.Directory.Exists(target.DownloadDirectory).ShouldBe(active);
        fs.Directory.Exists(Path.GetDirectoryName(target.DownloadDirectory)!).ShouldBe(active);
        (await dbContext.DownloadTaskPhotoImageFiles.Select(x => x.Id).OrderBy(x => x).ToListAsync(CancellationToken))
            .ShouldBe(photoFiles.Select(x => x.Id).Order());
        (await dbContext.DownloadTaskOtherVideoFiles.Select(x => x.Id).SingleAsync(CancellationToken)).ShouldBe(sibling.Id);
    }

    [Test]
    public async Task ShouldReturnFailedResult_WhenSecondCleanupPathResolutionFails()
    {
        // Arrange
        await SetupDatabase(64011, config => config.PlexPhotoLibraryCount = 1);
        var dbContext = IDbContext;
        var library = await dbContext.PlexLibraries.SingleAsync(x => x.Type == PlexMediaType.PhotoAlbum);
        var album = FakeData
            .GetDownloadTaskPhotoFolder(new Seed(64011))
            .RuleFor(x => x.Files, _ => FakeData.GetDownloadTaskPhotoImageFile(new Seed(64012)).Generate(1))
            .Generate();
        var target = album.Files.Single();
        foreach (var node in new DownloadTaskBase[] { album }.Concat(album.Files))
        {
            node.PlexServerId = library.PlexServerId;
            node.PlexLibraryId = library.Id;
        }
        dbContext.DownloadTaskPhotoFolders.Add(album);
        target.DirectoryMeta.DownloadRootPath = Mock.Container.Resolve<IPathProvider>().DefaultDownloadsDestinationFolder;
        target.DirectoryMeta.PhotoFolder = "shared/file";
        await dbContext.SaveChangesAsync(CancellationToken);
        var parent = Path.GetDirectoryName(target.DownloadDirectory)!;
        Mock.Mock<IPath>()
            .Setup(x => x.GetDirectoryName(target.DownloadFilePath))
            .Returns(target.DownloadDirectory)
            .Verifiable(Times.Once());
        Mock.Mock<IPath>()
            .Setup(x => x.GetDirectoryName(target.DownloadDirectory))
            .Throws(new InvalidOperationException("second path failure"))
            .Verifiable(Times.Once());
        Mock.Mock<IDirectory>().Setup(x => x.Exists(target.DownloadDirectory)).Returns(true).Verifiable(Times.Once());
        Mock.Mock<IDirectory>()
            .Setup(x => x.GetFileSystemEntries(target.DownloadDirectory))
            .Returns([])
            .Verifiable(Times.Once());
        Mock.Mock<IDirectory>().Setup(x => x.Delete(target.DownloadDirectory)).Verifiable(Times.Once());

        // Act
        var result = await TestHandlerExecuteAsync(new CleanUpDownloadTaskFoldersCommand(target.ToKey()));

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);
        result.HasException<InvalidOperationException>().ShouldBeTrue();
        Mock.Mock<IPath>().Verify();
        Mock.Mock<IDirectory>().Verify();
        Mock.Mock<IDirectory>().Verify(x => x.Exists(parent), Times.Never());
        Mock.Mock<IDirectory>().Verify(x => x.Delete(parent), Times.Never());
    }

    [Test]
    public async Task ShouldReturnFailedResult_WhenSecondCleanupDirectoryEntriesCannotBeRead()
    {
        // Arrange
        await SetupDatabase(64013, config => config.PlexPhotoLibraryCount = 1);
        var dbContext = IDbContext;
        var library = await dbContext.PlexLibraries.SingleAsync(x => x.Type == PlexMediaType.PhotoAlbum);
        var album = FakeData
            .GetDownloadTaskPhotoFolder(new Seed(64013))
            .RuleFor(x => x.Files, _ => FakeData.GetDownloadTaskPhotoImageFile(new Seed(64014)).Generate(1))
            .Generate();
        var target = album.Files.Single();
        foreach (var node in new DownloadTaskBase[] { album }.Concat(album.Files))
        {
            node.PlexServerId = library.PlexServerId;
            node.PlexLibraryId = library.Id;
        }
        dbContext.DownloadTaskPhotoFolders.Add(album);
        target.DirectoryMeta.DownloadRootPath = Mock.Container.Resolve<IPathProvider>().DefaultDownloadsDestinationFolder;
        target.DirectoryMeta.PhotoFolder = "shared/file";
        await dbContext.SaveChangesAsync(CancellationToken);
        var parent = Path.GetDirectoryName(target.DownloadDirectory)!;
        Mock.Mock<IPath>()
            .Setup(x => x.GetDirectoryName(target.DownloadFilePath))
            .Returns(target.DownloadDirectory)
            .Verifiable(Times.Once());
        Mock.Mock<IPath>()
            .Setup(x => x.GetDirectoryName(target.DownloadDirectory))
            .Returns(parent)
            .Verifiable(Times.Once());
        Mock.Mock<IDirectory>().Setup(x => x.Exists(target.DownloadDirectory)).Returns(true).Verifiable(Times.Once());
        Mock.Mock<IDirectory>()
            .Setup(x => x.GetFileSystemEntries(target.DownloadDirectory))
            .Returns([])
            .Verifiable(Times.Once());
        Mock.Mock<IDirectory>().Setup(x => x.Delete(target.DownloadDirectory)).Verifiable(Times.Once());
        Mock.Mock<IDirectory>().Setup(x => x.Exists(parent)).Returns(true).Verifiable(Times.Once());
        Mock.Mock<IDirectory>()
            .Setup(x => x.GetFileSystemEntries(parent))
            .Throws(new UnauthorizedAccessException("second directory denied"))
            .Verifiable(Times.Once());

        // Act
        var result = await TestHandlerExecuteAsync(new CleanUpDownloadTaskFoldersCommand(target.ToKey()));

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);
        result.HasException<UnauthorizedAccessException>().ShouldBeTrue();
        Mock.Mock<IPath>().Verify();
        Mock.Mock<IDirectory>().Verify();
        Mock.Mock<IDirectory>().Verify(x => x.Delete(target.DownloadDirectory), Times.Once());
        Mock.Mock<IDirectory>().Verify(x => x.Delete(parent), Times.Never());
    }

    [Test]
    public async Task ShouldReturnFailedResult_WhenSecondCleanupDirectoryCannotBeDeleted()
    {
        // Arrange
        await SetupDatabase(64015, config => config.PlexPhotoLibraryCount = 1);
        var dbContext = IDbContext;
        var library = await dbContext.PlexLibraries.SingleAsync(x => x.Type == PlexMediaType.PhotoAlbum);
        var album = FakeData
            .GetDownloadTaskPhotoFolder(new Seed(64015))
            .RuleFor(x => x.Files, _ => FakeData.GetDownloadTaskPhotoImageFile(new Seed(64016)).Generate(1))
            .Generate();
        var target = album.Files.Single();
        foreach (var node in new DownloadTaskBase[] { album }.Concat(album.Files))
        {
            node.PlexServerId = library.PlexServerId;
            node.PlexLibraryId = library.Id;
        }
        dbContext.DownloadTaskPhotoFolders.Add(album);
        target.DirectoryMeta.DownloadRootPath = Mock.Container.Resolve<IPathProvider>().DefaultDownloadsDestinationFolder;
        target.DirectoryMeta.PhotoFolder = "shared/file";
        await dbContext.SaveChangesAsync(CancellationToken);
        var parent = Path.GetDirectoryName(target.DownloadDirectory)!;
        Mock.Mock<IPath>()
            .Setup(x => x.GetDirectoryName(target.DownloadFilePath))
            .Returns(target.DownloadDirectory)
            .Verifiable(Times.Once());
        Mock.Mock<IPath>()
            .Setup(x => x.GetDirectoryName(target.DownloadDirectory))
            .Returns(parent)
            .Verifiable(Times.Once());
        Mock.Mock<IDirectory>().Setup(x => x.Exists(target.DownloadDirectory)).Returns(true).Verifiable(Times.Once());
        Mock.Mock<IDirectory>()
            .Setup(x => x.GetFileSystemEntries(target.DownloadDirectory))
            .Returns([])
            .Verifiable(Times.Once());
        Mock.Mock<IDirectory>().Setup(x => x.Delete(target.DownloadDirectory)).Verifiable(Times.Once());
        Mock.Mock<IDirectory>().Setup(x => x.Exists(parent)).Returns(true).Verifiable(Times.Once());
        Mock.Mock<IDirectory>()
            .Setup(x => x.GetFileSystemEntries(parent))
            .Returns([])
            .Verifiable(Times.Once());
        Mock.Mock<IDirectory>()
            .Setup(x => x.Delete(parent))
            .Throws(new UnauthorizedAccessException("second delete denied"))
            .Verifiable(Times.Once());

        // Act
        var result = await TestHandlerExecuteAsync(new CleanUpDownloadTaskFoldersCommand(target.ToKey()));

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);
        result.HasException<UnauthorizedAccessException>().ShouldBeTrue();
        Mock.Mock<IPath>().Verify();
        Mock.Mock<IDirectory>().Verify();
        Mock.Mock<IDirectory>().Verify(x => x.Delete(target.DownloadDirectory), Times.Once());
        Mock.Mock<IDirectory>().Verify(x => x.Delete(parent), Times.Once());
    }
    [Test]
    public async Task ShouldCleanStandalonePhotoImageFolderWithoutAlbum()
    {
        await SetupDatabase(88106, config => config.PlexPhotoLibraryCount = 1);
        var dbContext = IDbContext;
        var library = await dbContext.PlexLibraries.SingleAsync(x => x.Type == PlexMediaType.PhotoAlbum);
        var folder = FakeData.GetDownloadTaskPhotoFolder(new Seed(88106)).Generate();
        folder.PlexServerId = library.PlexServerId;
        folder.PlexLibraryId = library.Id;
        var file = folder.Files.Single();
        file.PlexServerId = library.PlexServerId;
        file.PlexLibraryId = library.Id;
        file.DownloadStatus = DownloadStatus.Completed;
        file.DirectoryMeta.DownloadRootPath = Mock.Container.Resolve<IPathProvider>().DefaultDownloadsDestinationFolder;
        file.DirectoryMeta.PhotoFolder = "root-1/folder";
        dbContext.DownloadTaskPhotoFolders.Add(folder);
        await dbContext.SaveChangesAsync(CancellationToken);
        SetupFileSystem(fs => fs.AddDirectory(file.DownloadDirectory));

        var result = await TestHandlerExecuteAsync(new CleanUpDownloadTaskFoldersCommand(file.ToKey()));

        result.IsSuccess.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        Mock.Container.Resolve<IFileSystem>().Directory.Exists(file.DownloadDirectory).ShouldBeFalse();
        (await dbContext.DownloadTaskPhotoFolders.SingleAsync(CancellationToken)).ParentId.ShouldBeNull();
        (await dbContext.DownloadTaskPhotoImageFiles.SingleAsync(CancellationToken)).Id.ShouldBe(file.Id);
    }
}
