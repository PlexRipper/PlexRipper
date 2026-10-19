namespace Reaparr.Application.UnitTests;

public class SyncPlexPhotosCommandUnitTests : BaseCommandUnitTest<SyncPlexPhotosCommand>
{
    [Test]
    public async Task ShouldPersistOverlappingAlbumMembershipsAndDirectFolderEdges_WithStableIdsOnRefresh()
    {
        // Arrange
        await SetupDatabase(73001, x => x.PlexPhotoLibraryCount = 1);
        var dbContext = IDbContext;
        var library = await dbContext.PlexLibraries.SingleAsync(CancellationToken);
        var root = CreateRootInput("/fixture/photos");
        var albumA = CreateAlbum(101, "Album A");
        var albumB = CreateAlbum(102, "Album B");
        var fileOne = CreateFile(201, 301, "/fixture/photos/Trips/one.jpg", "one.jpg");
        var fileTwo = CreateFile(202, 302, "/fixture/photos/two.jpg", "two.jpg");
        var firstSnapshot = CreateSnapshot(
            library,
            [albumA, albumB],
            [fileOne, fileTwo],
            [root],
            [new(101, 201), new(102, 201), new(101, 202)]
        );

        // Act
        var firstResult = await TestHandlerExecuteAsync<CrudPhotosReport>(new SyncPlexPhotosCommand(firstSnapshot));

        // Assert
        firstResult.IsSuccess.ShouldBeTrue(firstResult.ToString());
        firstResult.Errors.Count.ShouldBe(0);
        firstResult.Value.CreatedPhotos.ShouldBe(2);
        firstResult.Value.UpdatedPhotos.ShouldBe(0);
        firstResult.Value.UnchangedPhotos.ShouldBe(0);
        var persistedFileOne = await dbContext.PlexPhotoMediaData.SingleAsync(x => x.PlexApiRatingKey == 201, CancellationToken);
        var persistedFileTwo = await dbContext.PlexPhotoMediaData.SingleAsync(x => x.PlexApiRatingKey == 202, CancellationToken);
        var persistedAlbumA = await dbContext.PlexPhotoAlbums.SingleAsync(x => x.PlexApiRatingKey == 101, CancellationToken);
        var persistedAlbumB = await dbContext.PlexPhotoAlbums.SingleAsync(x => x.PlexApiRatingKey == 102, CancellationToken);
        (await dbContext.PlexPhotoAlbumMemberships
                .OrderBy(x => x.PlexPhotoAlbumId)
                .ThenBy(x => x.PlexPhotoMediaDataId)
                .Select(x => new { x.PlexPhotoAlbumId, x.PlexPhotoMediaDataId })
                .ToListAsync(CancellationToken))
            .ShouldBe([
                new { PlexPhotoAlbumId = persistedAlbumA.Id, PlexPhotoMediaDataId = persistedFileOne.Id },
                new { PlexPhotoAlbumId = persistedAlbumA.Id, PlexPhotoMediaDataId = persistedFileTwo.Id },
                new { PlexPhotoAlbumId = persistedAlbumB.Id, PlexPhotoMediaDataId = persistedFileOne.Id },
            ]);
        var rootFolder = await dbContext.PlexPhotoFolders.SingleAsync(x => x.ParentId == null, CancellationToken);
        var tripsFolder = await dbContext.PlexPhotoFolders.SingleAsync(x => x.RelativePath == "Trips", CancellationToken);
        persistedFileOne.PlexPhotoFolderId.ShouldBe(tripsFolder.Id);
        persistedFileTwo.PlexPhotoFolderId.ShouldBe(rootFolder.Id);

        var movedFileOne = CreateFile(201, 301, "/fixture/photos/Trips/Renamed/one.jpg", "one.jpg");
        var secondSnapshot = CreateSnapshot(
            library,
            [albumA, albumB],
            [movedFileOne, fileTwo],
            [root],
            [new(102, 201), new(101, 202)]
        );
        var secondResult = await TestHandlerExecuteAsync<CrudPhotosReport>(new SyncPlexPhotosCommand(secondSnapshot, true));
        secondResult.IsSuccess.ShouldBeTrue(secondResult.ToString());
        secondResult.Errors.Count.ShouldBe(0);
        secondResult.Value.CreatedPhotos.ShouldBe(0);
        secondResult.Value.UpdatedPhotos.ShouldBe(1);
        secondResult.Value.UnchangedPhotos.ShouldBe(1);

        var stableFileOne = await dbContext.PlexPhotoMediaData.SingleAsync(x => x.PlexApiRatingKey == 201, CancellationToken);
        var stableFileTwo = await dbContext.PlexPhotoMediaData.SingleAsync(x => x.PlexApiRatingKey == 202, CancellationToken);
        stableFileOne.Id.ShouldBe(persistedFileOne.Id);
        stableFileTwo.Id.ShouldBe(persistedFileTwo.Id);
        (await dbContext.PlexPhotoAlbums.OrderBy(x => x.PlexApiRatingKey).Select(x => new { x.PlexApiRatingKey, x.Id }).ToListAsync(CancellationToken))
            .ShouldBe([
                new { PlexApiRatingKey = 101, Id = persistedAlbumA.Id },
                new { PlexApiRatingKey = 102, Id = persistedAlbumB.Id },
            ]);
        var renamedFolder = await dbContext.PlexPhotoFolders.SingleAsync(x => x.RelativePath == "Trips/Renamed", CancellationToken);
        stableFileOne.PlexPhotoFolderId.ShouldBe(renamedFolder.Id);
        (await dbContext.PlexPhotoAlbumMemberships
                .OrderBy(x => x.PlexPhotoAlbumId)
                .ThenBy(x => x.PlexPhotoMediaDataId)
                .Select(x => new { x.PlexPhotoAlbumId, x.PlexPhotoMediaDataId })
                .ToListAsync(CancellationToken))
            .ShouldBe([
                new { PlexPhotoAlbumId = persistedAlbumA.Id, PlexPhotoMediaDataId = stableFileTwo.Id },
                new { PlexPhotoAlbumId = persistedAlbumB.Id, PlexPhotoMediaDataId = stableFileOne.Id },
            ]);
        var membershipOnlySnapshot = CreateSnapshot(
            library,
            [albumA, albumB],
            [movedFileOne, fileTwo],
            [root],
            [new(101, 201), new(101, 202)]
        );
        var membershipOnlyResult = await TestHandlerExecuteAsync<CrudPhotosReport>(
            new SyncPlexPhotosCommand(membershipOnlySnapshot, true)
        );
        membershipOnlyResult.IsSuccess.ShouldBeTrue(membershipOnlyResult.ToString());
        membershipOnlyResult.Value.UpdatedPhotos.ShouldBe(0);
        membershipOnlyResult.Value.UnchangedPhotos.ShouldBe(2);
        var membershipEdgesBeforeRepeat = await dbContext
            .PlexPhotoAlbumMemberships.OrderBy(x => x.PlexPhotoAlbumId)
            .ThenBy(x => x.PlexPhotoMediaDataId)
            .Select(x => new { x.PlexPhotoAlbumId, x.PlexPhotoMediaDataId })
            .ToListAsync(CancellationToken);
        membershipEdgesBeforeRepeat.ShouldBe([
            new { PlexPhotoAlbumId = persistedAlbumA.Id, PlexPhotoMediaDataId = stableFileOne.Id },
            new { PlexPhotoAlbumId = persistedAlbumA.Id, PlexPhotoMediaDataId = stableFileTwo.Id },
        ]);
        var repeatResult = await TestHandlerExecuteAsync<CrudPhotosReport>(
            new SyncPlexPhotosCommand(membershipOnlySnapshot, true)
        );
        repeatResult.IsSuccess.ShouldBeTrue(repeatResult.ToString());
        repeatResult.Value.UpdatedPhotos.ShouldBe(0);
        repeatResult.Value.UnchangedPhotos.ShouldBe(2);
        (await dbContext
                .PlexPhotoAlbumMemberships.OrderBy(x => x.PlexPhotoAlbumId)
                .ThenBy(x => x.PlexPhotoMediaDataId)
                .Select(x => new { x.PlexPhotoAlbumId, x.PlexPhotoMediaDataId })
                .ToListAsync(CancellationToken))
            .ShouldBe(membershipEdgesBeforeRepeat);
    }

    [Test]
    public async Task ShouldPreserveCatalogWhenSnapshotHasDuplicateFileIdentity()
    {
        // Arrange
        await SetupDatabase(73002, x => x.PlexPhotoLibraryCount = 1);
        var dbContext = IDbContext;
        var library = await dbContext.PlexLibraries.SingleAsync(CancellationToken);
        var root = CreateRootInput("/fixture/photos");
        var album = CreateAlbum(110, "Album");
        var file = CreateFile(210, 310, "/fixture/photos/one.jpg", "one.jpg");
        var validSnapshot = CreateSnapshot(library, [album], [file], [root], [new(110, 210)]);
        (await TestHandlerExecuteAsync<CrudPhotosReport>(new SyncPlexPhotosCommand(validSnapshot))).IsSuccess.ShouldBeTrue();
        var before = await dbContext.PlexPhotoMediaData.Select(x => new { x.Id, x.PlexApiRatingKey, x.Key }).SingleAsync(CancellationToken);
        var duplicate = CreateFile(210, 311, "/fixture/photos/other.jpg", "other.jpg");
        var invalidSnapshot = CreateSnapshot(library, [album], [file, duplicate], [root], [new(110, 210)]);

        // Act
        var result = await TestHandlerExecuteAsync<CrudPhotosReport>(new SyncPlexPhotosCommand(invalidSnapshot));

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.IsCancelled.ShouldBeFalse();
        result.Errors.Count.ShouldBe(1);
        (await dbContext.PlexPhotoMediaData.Select(x => new { x.Id, x.PlexApiRatingKey, x.Key }).SingleAsync(CancellationToken))
            .ShouldBe(before);
        (await dbContext.PlexPhotoAlbumMemberships.CountAsync(CancellationToken)).ShouldBe(1);
    }

    [Test]
    public async Task ShouldReusePositiveLocationRootsAcrossReorderAndPathRename()
    {
        // Arrange
        await SetupDatabase(73004, x => x.PlexPhotoLibraryCount = 1);
        var dbContext = IDbContext;
        var library = await dbContext.PlexLibraries.SingleAsync(CancellationToken);
        var firstRootA = CreateRootInput(34, "/fixture/photos");
        var firstRootB = CreateRootInput(35, "/fixture/other");
        var firstFileA = CreateFile(230, 330, "/fixture/photos/one.jpg", "one.jpg");
        var firstFileB = CreateFile(231, 331, "/fixture/other/two.jpg", "two.jpg");

        var firstResult = await TestHandlerExecuteAsync<CrudPhotosReport>(
            new SyncPlexPhotosCommand(
                CreateSnapshot(library, [], [firstFileA, firstFileB], [firstRootA, firstRootB], [])
            )
        );
        firstResult.IsSuccess.ShouldBeTrue(firstResult.ToString());
        var firstRoots = await dbContext
            .PlexPhotoFolders.Where(x => x.ParentId == null)
            .OrderBy(x => x.RootIdentityKey)
            .Select(x => new { x.Id, x.RootIdentityKey, x.SourceRootPath })
            .ToListAsync(CancellationToken);
        var firstFiles = await dbContext
            .PlexPhotoMediaData.OrderBy(x => x.PlexApiRatingKey)
            .Select(x => new { x.PlexApiRatingKey, x.PlexPhotoFolderId })
            .ToListAsync(CancellationToken);

        var renamedRootA = CreateRootInput(34, "/fixture/photos-renamed");
        var renamedRootB = CreateRootInput(35, "/fixture/other-renamed");
        var renamedFileA = CreateFile(230, 330, "/fixture/photos-renamed/one.jpg", "one.jpg");
        var renamedFileB = CreateFile(231, 331, "/fixture/other-renamed/two.jpg", "two.jpg");

        // Act
        var secondResult = await TestHandlerExecuteAsync<CrudPhotosReport>(
            new SyncPlexPhotosCommand(
                CreateSnapshot(
                    library,
                    [],
                    [renamedFileB, renamedFileA],
                    [renamedRootB, renamedRootA],
                    []
                )
            )
        );

        // Assert
        secondResult.IsSuccess.ShouldBeTrue(secondResult.ToString());
        (await dbContext
                .PlexPhotoFolders.Where(x => x.ParentId == null)
                .OrderBy(x => x.RootIdentityKey)
                .Select(x => new { x.Id, x.RootIdentityKey, x.SourceRootPath })
                .ToListAsync(CancellationToken))
            .ShouldBe([
                new { Id = firstRoots[0].Id, RootIdentityKey = (string?)"id:34", SourceRootPath = (string?)"/fixture/photos-renamed" },
                new { Id = firstRoots[1].Id, RootIdentityKey = (string?)"id:35", SourceRootPath = (string?)"/fixture/other-renamed" },
            ]);
        (await dbContext
                .PlexPhotoMediaData.OrderBy(x => x.PlexApiRatingKey)
                .Select(x => new { x.PlexApiRatingKey, x.PlexPhotoFolderId })
                .ToListAsync(CancellationToken))
            .ShouldBe(firstFiles);
    }

    [Test]
    public async Task ShouldRejectPositiveLocationIdentityAndConflictingSourceScopeBeforeWrites()
    {
        // Arrange
        await SetupDatabase(73005, x => x.PlexPhotoLibraryCount = 1);
        var dbContext = IDbContext;
        var library = await dbContext.PlexLibraries.SingleAsync(CancellationToken);
        var validRoot = CreateRootInput(34, "/fixture/photos");
        var validFile = CreateFile(240, 340, "/fixture/photos/one.jpg", "one.jpg");
        (await TestHandlerExecuteAsync<CrudPhotosReport>(
            new SyncPlexPhotosCommand(CreateSnapshot(library, [], [validFile], [validRoot], []))
        )).IsSuccess.ShouldBeTrue();
        var before = await dbContext
            .PlexPhotoMediaData.Select(x => new { x.Id, x.PlexApiRatingKey, x.Key })
            .SingleAsync(CancellationToken);

        var mismatchedRoot = new PhotoSourceRootInput(34, "path:posix:/fixture/photos", "/fixture/photos");
        var rootMismatchResult = await TestHandlerExecuteAsync<CrudPhotosReport>(
            new SyncPlexPhotosCommand(CreateSnapshot(library, [], [validFile], [mismatchedRoot], []))
        );

        var conflictingAlbum = CreateAlbum(241, "other-library-album");
        conflictingAlbum.PlexLibraryId = library.Id + 100;
        var conflictingFile = CreateFile(242, 342, "/fixture/photos/two.jpg", "two.jpg");
        conflictingFile.PlexServerId = library.PlexServerId + 100;

        // Act
        var scopeMismatchResult = await TestHandlerExecuteAsync<CrudPhotosReport>(
            new SyncPlexPhotosCommand(
                CreateSnapshot(
                    library,
                    [conflictingAlbum],
                    [conflictingFile],
                    [validRoot],
                    [new(241, 242)]
                )
            )
        );

        // Assert
        rootMismatchResult.IsFailed.ShouldBeTrue();
        scopeMismatchResult.IsFailed.ShouldBeTrue();
        (await dbContext
                .PlexPhotoMediaData.Select(x => new { x.Id, x.PlexApiRatingKey, x.Key })
                .SingleAsync(CancellationToken))
            .ShouldBe(before);
        (await dbContext.PlexPhotoAlbums.CountAsync(CancellationToken)).ShouldBe(0);
    }

    [Test]
    public async Task ShouldDeactivateRemovedRootsAndRetainUnmappedFilesWithoutChildFolders()
    {
        // Arrange
        await SetupDatabase(73003, x => x.PlexPhotoLibraryCount = 1);
        var dbContext = IDbContext;
        var library = await dbContext.PlexLibraries.SingleAsync(CancellationToken);
        var root = CreateRootInput("/fixture/photos");
        var file = CreateFile(220, 320, "/fixture/photos/Folder/file.jpg", "file.jpg");
        var initial = CreateSnapshot(library, [], [file], [root], []);
        (await TestHandlerExecuteAsync<CrudPhotosReport>(new SyncPlexPhotosCommand(initial))).IsSuccess.ShouldBeTrue();
        var rootId = await dbContext.PlexPhotoFolders.Where(x => x.ParentId == null).Select(x => x.Id).SingleAsync(CancellationToken);

        var unresolved = CreateFile(220, 320, "/outside/file.jpg", "file.jpg");
        var removedRoot = CreateSnapshot(library, [], [unresolved], [], []);

        // Act
        var result = await TestHandlerExecuteAsync<CrudPhotosReport>(new SyncPlexPhotosCommand(removedRoot));

        // Assert
        result.IsSuccess.ShouldBeTrue(result.ToString());
        var retainedRoot = await dbContext.PlexPhotoFolders.SingleAsync(x => x.Id == rootId, CancellationToken);
        retainedRoot.IsActive.ShouldBe((bool?)false);
        (await dbContext.PlexPhotoFolders.CountAsync(x => x.ParentId != null, CancellationToken)).ShouldBe(0);
        var retainedFile = await dbContext.PlexPhotoMediaData.SingleAsync(CancellationToken);
        retainedFile.PlexPhotoFolderId.ShouldBeNull();
        retainedFile.SourcePath.ShouldBe("/outside/file.jpg");
    }

    private static PhotoLibrarySnapshot CreateSnapshot(
        PlexLibrary databaseLibrary,
        IReadOnlyList<PlexPhotoAlbum> albums,
        IReadOnlyList<PlexPhotoMediaData> files,
        IReadOnlyList<PhotoSourceRootInput> roots,
        IReadOnlyList<PhotoAlbumMembershipInput> memberships
    )
    {
        var library = FakeData.GetPlexLibrary(new Seed(databaseLibrary.Id + 1), PlexMediaType.PhotoAlbum).Generate();
        library.Id = databaseLibrary.Id;
        library.PlexServerId = databaseLibrary.PlexServerId;
        library.Title = databaseLibrary.Title;
        library.ContentChangedAt = databaseLibrary.ContentChangedAt + 1;
        library.PhotoAlbums.AddRange(albums);
        library.PhotoMediaData.AddRange(files);
        return new PhotoLibrarySnapshot(library, roots, memberships);
    }

    private static PhotoSourceRootInput CreateRootInput(string path)
    {
        PhotoSourcePathResolver.TryCreateIdentityKey(path, out var identity, out var error).ShouldBeTrue(error);
        return new PhotoSourceRootInput(null, identity, path);
    }
    private static PhotoSourceRootInput CreateRootInput(long locationId, string path) =>
        new(locationId, $"id:{locationId}", path);


    private static PlexPhotoAlbum CreateAlbum(int ratingKey, string title) =>
        new()
        {
            Id = 0,
            PlexServerId = 1,
            PlexLibraryId = 1,
            PlexApiRatingKey = ratingKey,
            Title = title,
            SearchTitle = title.ToSearchTitle(),
            Summary = $"Summary {title}",
            PlexApiMetaDataKey = ratingKey,
            HasThumb = true,
            AddedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow,
        };

    private static PlexPhotoMediaData CreateFile(int ratingKey, int partId, string sourcePath, string filename) =>
        new()
        {
            Id = 0,
            PlexServerId = 1,
            PlexLibraryId = 1,
            PlexApiRatingKey = ratingKey,
            PlexApiMediaId = partId + 1000,
            PlexApiPartId = partId,
            AssetKind = PhotoAssetKind.Image,
            Title = filename,
            SearchTitle = filename.ToSearchTitle(),
            Summary = null,
            PlexApiMetaDataKey = ratingKey,
            HasThumb = true,
            AddedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow,
            OriginallyAvailableAt = DateTime.UtcNow.AddDays(-3),
            SourcePath = sourcePath,
            OriginalFilename = filename,
            Key = $"/library/parts/{partId}/file.jpg",
            Size = 100,
            Container = "jpg",
            Width = 100,
            Height = 100,
            Duration = null,
            VideoCodec = null,
            AudioCodec = null,
            PlexPhotoFolderId = null,
        };
}
