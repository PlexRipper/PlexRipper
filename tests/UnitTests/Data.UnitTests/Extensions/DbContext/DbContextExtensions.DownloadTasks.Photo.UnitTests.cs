namespace Reaparr.Data.UnitTests;

public class DbContextExtensionsDownloadTasksPhotoUnitTests : BaseUnitTest
{
    [Test]
    public async Task ShouldResolvePhotoKeysAndDetailsThroughAlbum_WhenSelectingAnyLevel()
    {
        // Arrange
        var seed = await SetupDatabase(62401, config => config.PlexPhotoLibraryCount = 1);
        var dbContext = IDbContext;
        var library = await dbContext.PlexLibraries.SingleAsync(CancellationToken);
        var album = (await AddAlbums(dbContext, library, seed, 1)).Single();
        var image = album.Children.Single();
        var file = image.Children.Single();
        var keys = new[] { album.ToKey(), image.ToKey(), file.ToKey() };
        // Act

        var found = await dbContext.GetDownloadTaskKeysAsync(
            [album.Id, image.Id, file.Id, Guid.Empty],
            CancellationToken
        );
        // Assert
        found.ShouldBe(keys);
        foreach (var key in keys)
        {
            (await dbContext.GetDownloadTaskKeyAsync(key.Id, CancellationToken)).ShouldBe(key);
            (await dbContext.GetDownloadTaskTypeAsync(key.Id, CancellationToken)).ShouldBe(key.Type);
            (await dbContext.GetRootDownloadTaskKeyAsync(key, cancellationToken: CancellationToken)).ShouldBe(
                album.ToKey()
            );
            var inferred = await dbContext.GetDownloadTaskAsync(key.Id, cancellationToken: CancellationToken);
            inferred.ShouldNotBeNull();
            inferred.Id.ShouldBe(key.Id);
            inferred.DownloadTaskType.ShouldBe(key.Type);
            inferred.ParentId.ShouldBe(
                key.Id == album.Id ? Guid.Empty : key.Id == image.Id ? album.Id : image.Id
            );
            (await dbContext.GetDownloadableChildTaskKeys(key, CancellationToken)).ShouldBe([file.ToKey()]);
        }

        album.ToParentKey().ShouldBeNull();
        image.ToParentKey().ShouldBe(album.ToKey());
        file.ToParentKey().ShouldBe(image.ToKey());
        DownloadTaskType.PhotoAlbum.ToPlexMediaType().ShouldBe(PlexMediaType.PhotoAlbum);
        DownloadTaskType.PhotoImage.ToPlexMediaType().ShouldBe(PlexMediaType.PhotoImage);
        DownloadTaskType.PhotoAlbum.ToDownloadTaskString().ToDownloadTaskType().ShouldBe(DownloadTaskType.PhotoAlbum);
        DownloadTaskType.PhotoImage.ToDownloadTaskString().ToDownloadTaskType().ShouldBe(DownloadTaskType.PhotoImage);

        var detail = await dbContext.GetDownloadTaskAsync(album.ToKey(), CancellationToken);
        detail.ShouldNotBeNull();
        detail.ParentId.ShouldBe(Guid.Empty);
        detail.MediaType.ShouldBe(PlexMediaType.PhotoAlbum);
        var imageDetail = detail.Children.Single();
        imageDetail.Id.ShouldBe(image.Id);
        imageDetail.ParentId.ShouldBe(album.Id);
        imageDetail.IsDownloadable.ShouldBeFalse();
        var fileDetail = imageDetail.Children.Single();
        fileDetail.Id.ShouldBe(file.Id);
        fileDetail.ParentId.ShouldBe(image.Id);
        fileDetail.IsDownloadable.ShouldBeTrue();
        fileDetail.MediaType.ShouldBe(PlexMediaType.PhotoImage);
        (await dbContext.GetDownloadableChildTaskKeys(album.ToKey(), CancellationToken)).ShouldBe([file.ToKey()]);
        (await dbContext.GetDownloadableChildTaskKeys(image.ToKey(), CancellationToken)).ShouldBe([file.ToKey()]);
        (await dbContext.GetAffectedRootDownloadTaskIdsAsync([image.Id, file.Id], CancellationToken)).ShouldBe([
            album.Id,
        ]);
    }

    [Test]
    public async Task ShouldReturnOnlyAlbumRootsWithNestedProgress_WhenFilteringByServer()
    {
        // Arrange
        var seed = await SetupDatabase(
            62402,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexPhotoLibraryCount = 1;
            }
        );
        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.PlexServerId).ToListAsync(CancellationToken);
        var album = (await AddAlbums(dbContext, libraries[0], seed, 1, 2)).Single();
        await AddAlbums(dbContext, libraries[1], seed, 1);
        var files = album.Children.SelectMany(x => x.Children).ToList();
        files[0].DataTotal = 1000;
        files[0].DataReceived = 250;
        files[0].Percentage = 25;
        files[0].DownloadStatus = DownloadStatus.Downloading;
        files[1].DataTotal = 2000;
        files[1].DataReceived = 2000;
        files[1].Percentage = 100;
        files[1].DownloadStatus = DownloadStatus.Completed;
        await dbContext.SaveChangesAsync(CancellationToken);

        // Act
        var progress = await dbContext.GetDownloadProgressTasksByServerAsync(
            libraries[0].PlexServerId,
            CancellationToken
        );
        // Assert
        var root = progress.Single();
        root.Id.ShouldBe(album.Id);
        root.DownloadTaskType.ShouldBe(DownloadTaskType.PhotoAlbum);
        root.DataTotal.ShouldBe(3000);
        root.DataReceived.ShouldBe(2250);
        root.Percentage.ShouldBe(62.5m);
        root.DownloadStatus.ShouldBe(DownloadStatus.Downloading);
        root.Children.Select(x => x.Id).Order().ShouldBe(album.Children.Select(x => x.Id).Order());
        root.Children.ShouldAllBe(x => x.ParentId == album.Id && !x.IsDownloadable);
        root.Children.SelectMany(x => x.Children).ShouldAllBe(x => x.IsDownloadable);
        root.Children.SelectMany(x => x.Children).Select(x => x.Id).Order().ShouldBe(files.Select(x => x.Id).Order());

        var details = await dbContext.GetAllDownloadTasksByServerAsync(
            libraries[0].PlexServerId,
            cancellationToken: CancellationToken
        );
        details.Select(x => x.Id).ShouldBe([album.Id]);
        details.Single().DataTotal.ShouldBe(root.DataTotal);
    }

    [Test]
    [Arguments(DownloadTaskType.PhotoData)]
    public async Task ShouldAggregateFileStatusThroughImageAndAlbum_WithoutChangingOtherAlbums(DownloadTaskType type)
    {
        // Arrange
        var seed = await SetupDatabase(62403, config => config.PlexPhotoLibraryCount = 1);
        var dbContext = IDbContext;
        var library = await dbContext.PlexLibraries.SingleAsync(CancellationToken);
        var albums = await AddAlbums(dbContext, library, seed, 2);
        var album = albums[0];
        var image = album.Children.Single();
        var file = image.Children.Single();
        var key = file.ToKey() with { Type = type };

        // Act
        await dbContext.SetDownloadStatus(key, DownloadStatus.Error);
        var changed = await dbContext.DetermineDownloadStatus(key, CancellationToken);
        // Assert

        changed.ShouldBe([image.ToKey(), album.ToKey()]);
        (await dbContext.GetDownloadStatusAsync(image.ToKey())).ShouldBe(DownloadStatus.Error);
        (await dbContext.GetDownloadTaskStatusAsync(album.ToKey(), CancellationToken)).ShouldBe(DownloadStatus.Error);
        (await dbContext.GetDownloadStatusAsync(albums[1].ToKey())).ShouldBe(DownloadStatus.Queued);
        (await dbContext.GetRootDownloadTaskKeyAsync(key, cancellationToken: CancellationToken)).ShouldBe(
            album.ToKey()
        );
        (await dbContext.GetDownloadTaskAsync(key, CancellationToken))!.Id.ShouldBe(file.Id);
    }

    [Test]
    [Arguments(DownloadTaskType.PhotoData)]
    public async Task ShouldPersistTransferAndResetPhotoProgress_WithoutLosingSourceOrParent(DownloadTaskType type)
    {
        // Arrange
        var seed = await SetupDatabase(62404, config => config.PlexPhotoLibraryCount = 1);
        var dbContext = IDbContext;
        var library = await dbContext.PlexLibraries.SingleAsync(CancellationToken);
        var album = (await AddAlbums(dbContext, library, seed, 1)).Single();
        var file = album.Children.Single().Children.Single();
        var key = file.ToKey() with { Type = type };
        file.DataTotal = 1000;
        file.DataReceived = 250;
        file.DownloadSpeed = 50;
        file.Percentage = 25;
        file.TimeRemaining = 15;
        file.FileTransferSpeed = 100;
        file.FileDataTransferred = 500;
        file.CurrentFileTransferBytesOffset = 500;

        // Act
        await dbContext.UpdateDownloadProgress(key, file, cancellationToken: CancellationToken);
        await dbContext.UpdateDownloadFileTransferProgress(key, file, CancellationToken);
        // Assert
        var persisted = await IDbContext.DownloadTaskPhotoImageFiles.SingleAsync(CancellationToken);
        persisted.DataReceived.ShouldBe(250);
        persisted.DataTotal.ShouldBe(1000);
        persisted.DownloadSpeed.ShouldBe(50);
        persisted.FileDataTransferred.ShouldBe(500);
        persisted.CurrentFileTransferBytesOffset.ShouldBe(500);
        persisted.Percentage.ShouldBe(50);

        await dbContext.ClearDownloadSpeed(key, CancellationToken);
        persisted = await IDbContext.DownloadTaskPhotoImageFiles.SingleAsync(CancellationToken);
        persisted.DownloadSpeed.ShouldBe(0);
        persisted.TimeRemaining.ShouldBe(0);
        persisted.FileTransferSpeed.ShouldBe(0);

        var reset = await dbContext.ResetDownloadTaskProgress(key, DownloadStatus.Stopped, CancellationToken);
        reset.IsSuccess.ShouldBeTrue();
        reset.Errors.Count.ShouldBe(0);
        persisted = await IDbContext.DownloadTaskPhotoImageFiles.SingleAsync(CancellationToken);
        persisted.DataReceived.ShouldBe(0);
        persisted.FileDataTransferred.ShouldBe(0);
        persisted.CurrentFileTransferBytesOffset.ShouldBe(0);
        persisted.FileTransferSpeed.ShouldBe(0);
        persisted.Percentage.ShouldBe(0);
        persisted.DownloadStatus.ShouldBe(DownloadStatus.Stopped);
        persisted.DataTotal.ShouldBe(1000);
        persisted.ParentId.ShouldBe(file.ParentId);
        persisted.FileLocationUrl.ShouldBe(file.FileLocationUrl);
        persisted.FileName.ShouldBe(file.FileName);
    }

    [Test]
    public async Task ShouldRemoveOnlyScopedEmptyImagesAndAlbums_WhenCleaningOrphans()
    {
        // Arrange
        var seed = await SetupDatabase(
            62405,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexPhotoLibraryCount = 1;
            }
        );
        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.OrderBy(x => x.PlexServerId).ToListAsync(CancellationToken);
        var album = (await AddAlbums(dbContext, libraries[0], seed, 1, 2)).Single();
        var unrelated = (await AddAlbums(dbContext, libraries[0], seed, 1)).Single();
        var otherServer = (await AddAlbums(dbContext, libraries[1], seed, 1)).Single();
        var images = album.Children.ToList();
        await dbContext
            .DownloadTaskPhotoImageFiles.Where(x => x.ParentId != images[1].Id)
            .ExecuteDeleteAsync(CancellationToken);

        // Act
        (await dbContext.DeleteOrphanedParentTasksByRootIdsAsync([album.Id], CancellationToken)).ShouldBe(1);
        // Assert
        (await dbContext.DownloadTaskPhotoImages.OrderBy(x => x.Id).Select(x => x.Id).ToListAsync(CancellationToken))
            .ShouldBe(new[] { images[1].Id, unrelated.Children.Single().Id, otherServer.Children.Single().Id }.Order());
        (await dbContext.DownloadTaskPhotoAlbums.OrderBy(x => x.Id).Select(x => x.Id).ToListAsync(CancellationToken))
            .ShouldBe(new[] { album.Id, unrelated.Id, otherServer.Id }.Order());

        (
            await dbContext.DeleteOrphanedParentTasksByServerIdAsync(libraries[0].PlexServerId, CancellationToken)
        ).ShouldBe(2);
        (await dbContext.DownloadTaskPhotoAlbums.OrderBy(x => x.Id).Select(x => x.Id).ToListAsync(CancellationToken))
            .ShouldBe(new[] { album.Id, otherServer.Id }.Order());
        (await dbContext.DeleteOrphanedParentTasksAsync(CancellationToken)).ShouldBe(2);
        (await dbContext.DownloadTaskPhotoAlbums.Select(x => x.Id).ToListAsync(CancellationToken)).ShouldBe([album.Id]);
    }

    [Test]
    public async Task ShouldCascadeAlbumDeletionThroughImagesFilesAndLogs_WithoutDeletingOtherAlbums()
    {
        // Arrange
        var seed = await SetupDatabase(62406, config => config.PlexPhotoLibraryCount = 1);
        var dbContext = IDbContext;
        var library = await dbContext.PlexLibraries.SingleAsync(CancellationToken);
        var albums = await AddAlbums(dbContext, library, seed, 2);
        foreach (var album in albums)
        {
            var file = album.Children.Single().Children.Single();
            await dbContext.CreateDownloadClientLog(
                file.ToKey(),
                NotificationLevel.Information,
                DownloadStatus.Queued,
                album.Title
            );
        }

        // Act
        await dbContext.DownloadTaskPhotoAlbums.Where(x => x.Id == albums[0].Id).ExecuteDeleteAsync(CancellationToken);

        // Assert
        (await dbContext.DownloadTaskPhotoImages.Select(x => x.ParentId).ToListAsync(CancellationToken)).ShouldBe([
            albums[1].Id,
        ]);
        var remainingFile = albums[1].Children.Single().Children.Single();
        (await dbContext.DownloadTaskPhotoImageFiles.Select(x => x.Id).ToListAsync(CancellationToken)).ShouldBe([
            remainingFile.Id,
        ]);
        var log = await dbContext.DownloadTaskPhotoImageFileLogs.SingleAsync(CancellationToken);
        log.DownloadTaskFileId.ShouldBe(remainingFile.Id);
        log.DownloadTaskPhotoId.ShouldBe(remainingFile.ParentId);
        log.DownloadTaskPhotoAlbumId.ShouldBe(albums[1].Id);
    }

    [Test]
    public async Task ShouldResolveStandaloneMultipartRootsWithoutPhantomAlbums_WhenSelectingImagesOrFiles()
    {
        // Arrange
        var seed = await SetupDatabase(62407, config =>
        {
            config.PhotoImageDownloadTasksCount = 2;
            config.PhotoImageFileDownloadTasksCount = 2;
        });
        var dbContext = IDbContext;
        var library = await dbContext.PlexLibraries.SingleAsync(CancellationToken);
        var images = await dbContext.DownloadTaskPhotoImages.Include(x => x.Children).OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var album = (await AddAlbums(dbContext, library, seed, 1)).Single();
        var target = images[0];
        var files = target.Children.OrderBy(x => x.Id).ToList();
        images.Select(x => x.ParentId).ShouldBe(new Guid?[] { null, null });
        files.Select(x => x.ParentId).ShouldBe([target.Id, target.Id]);

        // Act
        var roots = await dbContext.GetAllDownloadTasksByServerAsync(library.PlexServerId, cancellationToken: CancellationToken);
        var affected = await dbContext.GetAffectedRootDownloadTaskIdsAsync(
            [target.Id, files[0].Id, files[1].Id, album.Children.Single().Children.Single().Id], CancellationToken);

        // Assert
        roots.Select(x => x.Id).Order().ShouldBe(new[] { target.Id, images[1].Id, album.Id }.Order());
        affected.Order().ShouldBe(new[] { target.Id, album.Id }.Order());
        var detail = roots.Single(x => x.Id == target.Id);
        detail.ParentId.ShouldBe(Guid.Empty);
        detail.DownloadTaskType.ShouldBe(DownloadTaskType.PhotoImage);
        detail.Children.Select(x => (x.Id, x.ParentId, x.IsDownloadable)).OrderBy(x => x.Id)
            .ShouldBe(files.Select(x => (x.Id, target.Id, true)).OrderBy(x => x.Item1));
        foreach (var key in files.Select(x => x.ToKey()).Prepend(target.ToKey()))
        {
            (await dbContext.GetRootDownloadTaskKeyAsync(key, cancellationToken: CancellationToken)).ShouldBe(target.ToKey());
            (await dbContext.GetDownloadableChildTaskKeys(key, CancellationToken)).OrderBy(x => x.Id)
                .ShouldBe((key.Type == DownloadTaskType.PhotoData ? files.Where(x => x.Id == key.Id) : files)
                    .Select(x => x.ToKey()).OrderBy(x => x.Id));
        }
        (await dbContext.DownloadTaskPhotoImages.Where(x => x.Id == target.Id).ProjectToParentKey().ToListAsync(CancellationToken))
            .ShouldBeEmpty();
        (await dbContext.DownloadTaskPhotoAlbums.Select(x => x.Id).ToListAsync(CancellationToken)).ShouldBe([album.Id]);
    }

    [Test]
    public async Task ShouldAggregateStandaloneFileStatusOnlyIntoImage_WhenAnotherRootExists()
    {
        // Arrange
        await SetupDatabase(62408, config =>
        {
            config.PhotoImageDownloadTasksCount = 2;
            config.PhotoImageFileDownloadTasksCount = 2;
        });
        var dbContext = IDbContext;
        var images = await dbContext.DownloadTaskPhotoImages.Include(x => x.Children).OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var target = images[0];
        var file = target.Children.OrderBy(x => x.Id).First();
        images.Select(x => x.DownloadStatus).ShouldBe([DownloadStatus.Queued, DownloadStatus.Queued]);

        // Act
        await dbContext.SetDownloadStatus(file.ToKey(), DownloadStatus.Error);
        var changed = await dbContext.DetermineDownloadStatus(file.ToKey(), CancellationToken);

        // Assert
        changed.ShouldBe([target.ToKey()]);
        (await dbContext.DownloadTaskPhotoImages.OrderBy(x => x.Id)
            .Select(x => new { x.Id, x.ParentId, x.DownloadStatus }).ToListAsync(CancellationToken))
            .ShouldBe([
                new { Id = target.Id, ParentId = (Guid?)null, DownloadStatus = DownloadStatus.Error },
                new { Id = images[1].Id, ParentId = (Guid?)null, DownloadStatus = DownloadStatus.Queued },
            ]);
        (await dbContext.DetermineDownloadStatus(file.ToKey(), CancellationToken)).ShouldBeEmpty();
        (await dbContext.GetRootDownloadTaskKeyAsync(file.ToKey(), cancellationToken: CancellationToken)).ShouldBe(target.ToKey());
        (await dbContext.DownloadTaskPhotoAlbums.CountAsync(CancellationToken)).ShouldBe(0);
        (await dbContext.DownloadTaskPhotoImageFiles.Where(x => x.ParentId == images[1].Id)
            .Select(x => x.DownloadStatus).ToListAsync(CancellationToken)).ShouldBe([DownloadStatus.Queued, DownloadStatus.Queued]);
    }

    [Test]
    public async Task ShouldDeleteOnlySelectedEmptyStandaloneWrappers_WhenCleaningRootOrphans()
    {
        // Arrange
        var seed = await SetupDatabase(62409, config =>
        {
            config.PhotoImageDownloadTasksCount = 2;
            config.PhotoImageFileDownloadTasksCount = 2;
        });
        var dbContext = IDbContext;
        var library = await dbContext.PlexLibraries.SingleAsync(CancellationToken);
        var images = await dbContext.DownloadTaskPhotoImages.Include(x => x.Children).OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var album = (await AddAlbums(dbContext, library, seed, 1)).Single();
        var target = images[0];
        await dbContext.DownloadTaskPhotoImageFiles.Where(x => x.ParentId == target.Id).ExecuteDeleteAsync(CancellationToken);
        (await dbContext.DownloadTaskPhotoImages.OrderBy(x => x.Id).Select(x => x.Id).ToListAsync(CancellationToken))
            .ShouldBe(new[] { images[0].Id, images[1].Id, album.Children.Single().Id }.Order());

        // Act
        var deleted = await dbContext.DeleteOrphanedParentTasksByRootIdsAsync([target.Id], CancellationToken);

        // Assert
        deleted.ShouldBe(1);
        (await dbContext.DownloadTaskPhotoImages.OrderBy(x => x.Id).Select(x => x.Id).ToListAsync(CancellationToken))
            .ShouldBe(new[] { images[1].Id, album.Children.Single().Id }.Order());
        (await dbContext.DownloadTaskPhotoImageFiles.OrderBy(x => x.Id).Select(x => x.Id).ToListAsync(CancellationToken))
            .ShouldBe(images[1].Children.Concat(album.Children.Single().Children).Select(x => x.Id).Order());
        (await dbContext.DownloadTaskPhotoAlbums.Select(x => x.Id).ToListAsync(CancellationToken)).ShouldBe([album.Id]);
    }

    private async Task<List<DownloadTaskPhotoAlbum>> AddAlbums(
        IReaparrDbContext dbContext,
        PlexLibrary library,
        Seed seed,
        int albumCount,
        int imageCount = 1
    )
    {
        var albums = FakeData
            .GetDownloadTaskPhotoAlbum(seed)
            .RuleFor(x => x.Children, _ => FakeData.GetDownloadTaskPhotoImage(seed).Generate(imageCount))
            .Generate(albumCount);
        albums.SetRelationshipIds(library.PlexServerId, library.Id);
        dbContext.DownloadTaskPhotoAlbums.AddRange(albums);
        await dbContext.SaveChangesAsync(CancellationToken);
        return albums;
    }
}
