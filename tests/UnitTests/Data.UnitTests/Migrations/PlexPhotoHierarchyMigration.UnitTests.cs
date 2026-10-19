using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Reaparr.Data.UnitTests;

public class PlexPhotoHierarchyMigrationUnitTests : BaseUnitTest
{
    [Test]
    public async Task ShouldPreserveUnrelatedMovieData_WhenUpgradingThePrecedingSchema()
    {
        // Arrange
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync(CancellationToken);
        DbContextConnections.ConfigureOpenedSqliteConnection(connection);
        var options = new DbContextOptionsBuilder<ReaparrDbContext>().UseSqlite(connection).Options;
        await using var context = new ReaparrDbContext(options, Log, new MockPathProvider("photo-upgrade"), new MockAppRuntimeInfo());
        var migrator = context.GetService<IMigrator>();
        await migrator.MigrateAsync("20261017225840_TEMP4", CancellationToken);
        var server = FakeData.GetPlexServer(new Seed(42001)).Generate();
        context.PlexServers.Add(server);
        await context.SaveChangesAsync(CancellationToken);
        var library = FakeData.GetPlexLibrary(new Seed(42002), PlexMediaType.Movie).Generate();
        library.PlexServerId = server.Id;
        context.PlexLibraries.Add(library);
        await context.SaveChangesAsync(CancellationToken);
        var movie = FakeData.GetPlexMovies(new Seed(42003)).Generate();
        movie.PlexLibraryId = library.Id;
        movie.PlexServerId = server.Id;
        foreach (var part in movie.MediaDataList)
        {
            part.PlexLibraryId = library.Id;
            part.PlexServerId = server.Id;
        }
        context.PlexMovies.Add(movie);
        await context.SaveChangesAsync(CancellationToken);
        var before = await context.PlexMovies.IgnoreQueryFilters().Where(x => x.Id == movie.Id)
            .Select(x => new { x.Id, x.PlexApiRatingKey, x.Title, x.Year, x.Summary, x.PlexLibraryId, x.PlexServerId })
            .SingleAsync(CancellationToken);
        var partsBefore = await context.PlexMovieData.IgnoreQueryFilters().Where(x => x.PlexMovieId == movie.Id)
            .OrderBy(x => x.Id).Select(x => new { x.Id, x.PlexApiPartId, x.Size, x.Key }).ToListAsync(CancellationToken);
        partsBefore.Count.ShouldBe(1);
        var photoLibrary = FakeData.GetPlexLibrary(new Seed(42004), PlexMediaType.PhotoAlbum)
            .RuleFor(x => x.PhotoAlbumCount, 1)
            .RuleFor(x => x.PhotoImageCount, 1)
            .RuleFor(x => x.MediaSize, 100)
            .RuleFor(x => x.SyncedAt, new DateTime(2026, 10, 17, 0, 0, 0, DateTimeKind.Utc))
            .RuleFor(x => x.SyncedContentChangedAt, 1L)
            .RuleFor(x => x.Outdated, false)
            .Generate();
        photoLibrary.PlexServerId = server.Id;
        context.PlexLibraries.Add(photoLibrary);
        await context.SaveChangesAsync(CancellationToken);
        await using (var legacy = connection.CreateCommand())
        {
            legacy.CommandText = """
                INSERT INTO PlexPhotoAlbums
                    (Id,PlexApiRatingKey,Title,Year,SortIndex,SearchTitle,Duration,MediaSize,PlexApiMetaDataKey,
                     Studio,Summary,ContentRating,Rating,ChildCount,AddedAt,VideoQuality,HasThumb,HasArt,HasTheme,
                     FullTitle,Guid,PlexLibraryId,PlexServerId)
                VALUES (10,100,'Unreleased album',2026,0,'album',0,100,100,'','','',0,1,'2026-10-17',0,0,0,0,
                    'Unreleased album','album-100',$library,$server);
                INSERT INTO PlexPhotoImages
                    (Id,PlexApiRatingKey,Title,Year,SortIndex,SearchTitle,Duration,MediaSize,PlexApiMetaDataKey,
                     Studio,Summary,ContentRating,Rating,ChildCount,AddedAt,VideoQuality,HasThumb,HasArt,HasTheme,
                     FullTitle,Guid,ParentKey,PlexPhotoAlbumId,PlexLibraryId,PlexServerId)
                VALUES (11,101,'Image',2026,0,'image',0,100,101,'','','',0,0,'2026-10-17',0,0,0,0,
                    'Image','image-101',100,10,$library,$server);
                INSERT INTO PlexPhotoData
                    (Id,PlexApiRatingKey,PlexApiMediaId,PlexApiPartId,OriginalFilename,Container,
                     Duration,Size,Key,Source,VideoCodec,AudioCodec,NeedsGeneratedName,PlexPhotoId,VideoQuality,
                     PlexLibraryId,PlexServerId)
                VALUES (21,101,201,301,'image.jpg','jpg',0,100,'/library/parts/301',0,'','',0,11,0,$library,$server);
                INSERT INTO DownloadTaskPhotoAlbums
                    (Id,PlexApiRatingKey,Title,DownloadStatus,CreatedAt,FullTitle,PlexServerId,PlexLibraryId,Year)
                VALUES ($album,100,'Unreleased album','Queued','2026-10-17','Unreleased album',$server,$library,2026);
                INSERT INTO DownloadTaskPhotoImages
                    (Id,PlexApiRatingKey,Title,DownloadStatus,CreatedAt,FullTitle,PlexServerId,PlexLibraryId,Year,ParentId)
                VALUES ($image,101,'Image','Queued','2026-10-17','Image',$server,$library,2026,$album);
                INSERT INTO DownloadTaskPhotoImageFiles
                    (Id,PlexApiRatingKey,Title,DownloadStatus,CreatedAt,FullTitle,PlexServerId,PlexLibraryId,PlexApiMediaId,
                     PlexApiPartId,FileName,FileLocationUrl,VideoQuality,DirectoryMeta,DataReceived,DataTotal,DownloadSpeed,
                     FileTransferSpeed,FileDataTransferred,CurrentFileTransferBytesOffset,Percentage,TimeRemaining,ParentId)
                VALUES ($file,101,'Image','Queued','2026-10-17','image.jpg',$server,$library,201,301,'image.jpg',
                    '/library/parts/301',0,'{}',0,100,0,0,0,0,'0',0,$image);
                INSERT INTO DownloadTaskPhotoImageFileLogs
                    (Id,Status,LogLevel,Message,CreatedAt,DownloadTaskFileId,DownloadTaskPhotoId,DownloadTaskPhotoAlbumId)
                VALUES (41,'Queued','Information','Unreleased queue','2026-10-17',$file,$image,$album);
                """;
            legacy.Parameters.AddWithValue("$library", photoLibrary.Id);
            legacy.Parameters.AddWithValue("$server", server.Id);
            legacy.Parameters.AddWithValue("$album", Guid.Parse("11111111-1111-1111-1111-111111111111"));
            legacy.Parameters.AddWithValue("$image", Guid.Parse("22222222-2222-2222-2222-222222222222"));
            legacy.Parameters.AddWithValue("$file", Guid.Parse("33333333-3333-3333-3333-333333333333"));
            await legacy.ExecuteNonQueryAsync(CancellationToken);
            legacy.CommandText = "SELECT COUNT(*) FROM DownloadTaskPhotoImageFiles";
            ((long)(await legacy.ExecuteScalarAsync(CancellationToken))!).ShouldBe(1L);
        }

        // Act
        await migrator.MigrateAsync(cancellationToken: CancellationToken);

        // Assert
        (await context.PlexMovies.IgnoreQueryFilters().Where(x => x.Id == movie.Id)
            .Select(x => new { x.Id, x.PlexApiRatingKey, x.Title, x.Year, x.Summary, x.PlexLibraryId, x.PlexServerId })
            .SingleAsync(CancellationToken)).ShouldBe(before);
        (await context.PlexMovieData.IgnoreQueryFilters().Where(x => x.PlexMovieId == movie.Id)
            .OrderBy(x => x.Id).Select(x => new { x.Id, x.PlexApiPartId, x.Size, x.Key }).ToListAsync(CancellationToken))
            .ShouldBe(partsBefore);
        var resetLibrary = await context.PlexLibraries.IgnoreQueryFilters().AsNoTracking()
            .SingleAsync(x => x.Id == photoLibrary.Id, CancellationToken);
        resetLibrary.PhotoAlbumCount.ShouldBe(0);
        resetLibrary.PhotoImageCount.ShouldBe(0);
        resetLibrary.PhotoClipCount.ShouldBe(0);
        resetLibrary.MediaSize.ShouldBe(0L);
        resetLibrary.SyncedAt.ShouldBeNull();
        resetLibrary.SyncedContentChangedAt.ShouldBeNull();
        resetLibrary.Outdated.ShouldBeTrue();
        (await context.PlexPhotoAlbums.IgnoreQueryFilters().AnyAsync(CancellationToken)).ShouldBeFalse();
        (await context.PlexPhotoMediaData.IgnoreQueryFilters().AnyAsync(CancellationToken)).ShouldBeFalse();
        (await context.DownloadTaskPhotoFolders.IgnoreQueryFilters().AnyAsync(CancellationToken)).ShouldBeFalse();
        (await context.DownloadTaskPhotoImageFiles.IgnoreQueryFilters().AnyAsync(CancellationToken)).ShouldBeFalse();
        (await context.DownloadTaskPhotoImageFileLogs.IgnoreQueryFilters().AnyAsync(CancellationToken)).ShouldBeFalse();
        (await context.Database.GetPendingMigrationsAsync(CancellationToken)).ShouldBeEmpty();
        await using var check = connection.CreateCommand();
        check.CommandText = "PRAGMA foreign_key_check";
        await using var reader = await check.ExecuteReaderAsync(CancellationToken);
        (await reader.ReadAsync(CancellationToken)).ShouldBeFalse();
    }

    [Test]
    public async Task ShouldCreateMergedTablesAndPreserveFiles_WhenDeletingAlbumsOrCatalogFolders()
    {
        // Arrange
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync(CancellationToken);
        DbContextConnections.ConfigureOpenedSqliteConnection(connection);
        var options = new DbContextOptionsBuilder<ReaparrDbContext>()
            .UseSqlite(connection).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).Options;
        await using var context = new ReaparrDbContext(options, Log, new MockPathProvider("photo-fresh"), new MockAppRuntimeInfo());

        // Act
        await context.GetService<IMigrator>().MigrateAsync(cancellationToken: CancellationToken);

        // Assert
        await using var schema = connection.CreateCommand();
        schema.CommandText = """
            SELECT name FROM sqlite_master WHERE type='table' AND name IN (
                'PlexPhotoAlbums','PlexPhotoMediaData','PlexPhotoAlbumMemberships','PlexPhotoFolders',
                'DownloadTaskPhotoFolders','DownloadTaskPhotoImageFiles','DownloadTaskPhotoImageFileLogs',
                'PlexPhotoImages','PlexPhotoData','PlexPhotoSourceRoots','PlexPhotoFolderMemberships',
                'DownloadTaskPhotoAlbums','DownloadTaskPhotoImages') ORDER BY name
            """;
        var tables = new List<string>();
        await using (var reader = await schema.ExecuteReaderAsync(CancellationToken))
            while (await reader.ReadAsync(CancellationToken))
                tables.Add(reader.GetString(0));
        tables.ShouldBe([
            "DownloadTaskPhotoFolders", "DownloadTaskPhotoImageFileLogs", "DownloadTaskPhotoImageFiles",
            "PlexPhotoAlbumMemberships", "PlexPhotoAlbums", "PlexPhotoFolders", "PlexPhotoMediaData",
        ]);
        var model = context.GetService<Microsoft.EntityFrameworkCore.Metadata.IDesignTimeModel>().Model;
        var folderEntity = model.FindEntityType(typeof(PlexPhotoFolder))!;
        folderEntity.FindProperty(nameof(PlexPhotoFolder.RootIdentityKey))!.GetCollation().ShouldBe("BINARY");
        folderEntity.FindProperty(nameof(PlexPhotoFolder.RelativePath))!.GetCollation().ShouldBe("BINARY");
        typeof(BasePlexMedia).IsAssignableFrom(typeof(PlexPhotoAlbum)).ShouldBeFalse();
        typeof(BasePlexMedia).IsAssignableFrom(typeof(PlexPhotoMediaData)).ShouldBeFalse();
        var server = FakeData.GetPlexServer(new Seed(42006)).Generate();
        context.PlexServers.Add(server);
        await context.SaveChangesAsync(CancellationToken);
        var library = FakeData.GetPlexLibrary(new Seed(42007), PlexMediaType.PhotoAlbum).Generate();
        library.PlexServerId = server.Id;
        context.PlexLibraries.Add(library);
        await context.SaveChangesAsync(CancellationToken);
        var roots = new[] { "/fixture/A", "/fixture/a" }.Select(path => new PlexPhotoFolder
        {
            PlexLibraryId = library.Id, PlexServerId = server.Id, RelativePath = string.Empty, Title = path,
            RootIdentityKey = $"path:posix:{path}", SourceRootPath = path, IsActive = true,
        }).ToList();
        context.PlexPhotoFolders.AddRange(roots);
        await context.SaveChangesAsync(CancellationToken);
        context.PlexPhotoFolders.Add(new PlexPhotoFolder
        {
            PlexLibraryId = library.Id, PlexServerId = server.Id, RelativePath = string.Empty, Title = "duplicate",
            RootIdentityKey = roots[0].RootIdentityKey, SourceRootPath = "/fixture/renamed", IsActive = true,
        });
        var duplicateRoot = await Should.ThrowAsync<DbUpdateException>(async () =>
        {
            await context.SaveChangesAsync(CancellationToken);
        });
        duplicateRoot.InnerException.ShouldBeOfType<SqliteException>().SqliteExtendedErrorCode.ShouldBe(2067);
        context.ChangeTracker.Clear();
        context.PlexPhotoFolders.Add(new PlexPhotoFolder
        {
            PlexLibraryId = library.Id, PlexServerId = server.Id, ParentId = roots[0].Id,
            RelativePath = "invalid", Title = "invalid", SourceRootPath = "/fixture/root-only",
        });
        var invalidChild = await Should.ThrowAsync<DbUpdateException>(async () =>
        {
            await context.SaveChangesAsync(CancellationToken);
        });
        invalidChild.InnerException.ShouldBeOfType<SqliteException>().SqliteExtendedErrorCode.ShouldBe(275);
        context.ChangeTracker.Clear();
        (await context.PlexPhotoFolders.OrderBy(x => x.Id).Select(x => x.Id).ToListAsync(CancellationToken))
            .ShouldBe(roots.Select(x => x.Id));
        var children = new[] { "A", "a" }.Select(path => new PlexPhotoFolder
        {
            PlexLibraryId = library.Id, PlexServerId = server.Id, ParentId = roots[0].Id,
            RelativePath = path, Title = path,
        }).ToList();
        context.PlexPhotoFolders.AddRange(children);
        await context.SaveChangesAsync(CancellationToken);
        var files = FakeData.GetPlexPhotoMediaData(new Seed(42008)).Generate(2);
        files[0].PlexPhotoFolderId = children[0].Id;
        files[1].PlexPhotoFolderId = roots[1].Id;
        foreach (var file in files)
        {
            file.PlexLibraryId = library.Id;
            file.PlexServerId = server.Id;
        }
        context.PlexPhotoMediaData.AddRange(files);
        var albums = FakeData.GetPlexPhotoAlbums(new Seed(42009)).Generate(2);
        foreach (var album in albums)
        {
            album.PlexLibraryId = library.Id;
            album.PlexServerId = server.Id;
        }
        context.PlexPhotoAlbums.AddRange(albums);
        await context.SaveChangesAsync(CancellationToken);
        context.PlexPhotoAlbumMemberships.AddRange(albums.Select(album => new PlexPhotoAlbumMembership
        {
            PlexPhotoAlbumId = album.Id, PlexPhotoMediaDataId = files[0].Id,
        }));
        await context.SaveChangesAsync(CancellationToken);
        (await context.PlexPhotoAlbumMemberships.OrderBy(x => x.PlexPhotoAlbumId)
            .Select(x => new { x.PlexPhotoAlbumId, x.PlexPhotoMediaDataId }).ToListAsync(CancellationToken))
            .ShouldBe(albums.Select(x => new { PlexPhotoAlbumId = x.Id, PlexPhotoMediaDataId = files[0].Id }));
        (await context.PlexPhotoFolders.Where(x => x.ParentId == roots[0].Id).OrderBy(x => x.Id)
            .Select(x => x.RelativePath).ToListAsync(CancellationToken)).ShouldBe(["A", "a"]);

        await context.PlexPhotoAlbums.Where(x => x.Id == albums[0].Id).ExecuteDeleteAsync(CancellationToken);
        (await context.PlexPhotoAlbumMemberships.Select(x => new { x.PlexPhotoAlbumId, x.PlexPhotoMediaDataId })
            .SingleAsync(CancellationToken)).ShouldBe(new { PlexPhotoAlbumId = albums[1].Id, PlexPhotoMediaDataId = files[0].Id });
        (await context.PlexPhotoMediaData.OrderBy(x => x.Id).Select(x => x.Id).ToListAsync(CancellationToken))
            .ShouldBe(files.Select(x => x.Id));
        await context.PlexPhotoFolders.Where(x => x.Id == roots[0].Id).ExecuteDeleteAsync(CancellationToken);
        (await context.PlexPhotoFolders.Select(x => x.Id).ToListAsync(CancellationToken)).ShouldBe([roots[1].Id]);
        (await context.PlexPhotoMediaData.OrderBy(x => x.Id).Select(x => new { x.Id, x.PlexPhotoFolderId })
            .ToListAsync(CancellationToken)).ShouldBe([
                new { files[0].Id, PlexPhotoFolderId = (int?)null },
                new { files[1].Id, PlexPhotoFolderId = (int?)roots[1].Id },
            ]);
        (await context.Database.GetPendingMigrationsAsync(CancellationToken)).ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldCascadeOnlySelectedFolderFilesAndTheirLogs_WhenDeletingDirectQueueGroups()
    {
        // Arrange
        await using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync(CancellationToken);
        DbContextConnections.ConfigureOpenedSqliteConnection(connection);
        var options = new DbContextOptionsBuilder<ReaparrDbContext>()
            .UseSqlite(connection).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).Options;
        await using var context = new ReaparrDbContext(options, Log, new MockPathProvider("photo-queue"), new MockAppRuntimeInfo());
        await context.GetService<IMigrator>().MigrateAsync(cancellationToken: CancellationToken);
        var server = FakeData.GetPlexServer(new Seed(42010)).Generate();
        context.PlexServers.Add(server);
        await context.SaveChangesAsync(CancellationToken);
        var library = FakeData.GetPlexLibrary(new Seed(42011), PlexMediaType.PhotoAlbum).Generate();
        library.PlexServerId = server.Id;
        context.PlexLibraries.Add(library);
        await context.SaveChangesAsync(CancellationToken);
        var folder = FakeData.GetDownloadTaskPhotoFolder(new Seed(42012), config => config.PhotoImageDownloadTasksCount = 2).Generate();
        folder.PlexServerId = server.Id;
        folder.PlexLibraryId = library.Id;
        var control = FakeData.GetDownloadTaskPhotoFolder(new Seed(42013), config => config.PhotoImageDownloadTasksCount = 1).Generate();
        control.PlexServerId = server.Id;
        control.PlexLibraryId = library.Id;
        control.DestinationRootPath = "/Photos/control";
        foreach (var file in folder.Files.Concat(control.Files))
        {
            file.PlexServerId = server.Id;
            file.PlexLibraryId = library.Id;
        }
        context.DownloadTaskPhotoFolders.AddRange(folder, control);
        await context.SaveChangesAsync(CancellationToken);
        var files = folder.Files.OrderBy(x => x.Id).ToList();
        files.Count.ShouldBe(2);
        var allFiles = files.Concat(control.Files).OrderBy(x => x.Id).ToList();
        context.DownloadTaskPhotoImageFileLogs.AddRange(allFiles.Select(file => new DownloadTaskPhotoImageFileLog
        {
            DownloadTaskFileId = file.Id, Status = DownloadStatus.Queued, LogLevel = NotificationLevel.Information,
            Message = file.Title, CreatedAt = new DateTime(2026, 10, 17, 0, 0, 0, DateTimeKind.Utc),
        }));
        await context.SaveChangesAsync(CancellationToken);
        (await context.DownloadTaskPhotoImageFiles.OrderBy(x => x.Id).Select(x => new { x.Id, x.ParentId })
            .ToListAsync(CancellationToken)).ShouldBe(allFiles.Select(x => new { x.Id, x.ParentId }));

        // Act
        await context.DownloadTaskPhotoImageFiles.Where(x => x.Id == files[0].Id).ExecuteDeleteAsync(CancellationToken);
        var logsAfterFileDeletion = await context.DownloadTaskPhotoImageFileLogs.OrderBy(x => x.DownloadTaskFileId)
            .Select(x => x.DownloadTaskFileId).ToListAsync(CancellationToken);
        await context.DownloadTaskPhotoFolders.Where(x => x.Id == folder.Id).ExecuteDeleteAsync(CancellationToken);

        // Assert
        logsAfterFileDeletion.ShouldBe(allFiles.Where(x => x.Id != files[0].Id).Select(x => x.Id).OrderBy(x => x));
        (await context.DownloadTaskPhotoFolders.Select(x => x.Id).ToListAsync(CancellationToken)).ShouldBe([control.Id]);
        var retainedFileId = control.Files.Single().Id;
        (await context.DownloadTaskPhotoImageFiles.Select(x => x.Id).ToListAsync(CancellationToken)).ShouldBe([retainedFileId]);
        (await context.DownloadTaskPhotoImageFileLogs.Select(x => x.DownloadTaskFileId).ToListAsync(CancellationToken))
            .ShouldBe([retainedFileId]);
        await using var check = connection.CreateCommand();
        check.CommandText = "PRAGMA foreign_key_check";
        await using var reader = await check.ExecuteReaderAsync(CancellationToken);
        (await reader.ReadAsync(CancellationToken)).ShouldBeFalse();
    }
}
