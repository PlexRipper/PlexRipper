using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Reaparr.Data.UnitTests;

public class SyncedMediaMetadataMigrationUnitTests : BaseUnitTest
{
    [Test]
    public async Task ShouldPreserveAncestorValues_WhenUpgradingShowWithoutEpisodes()
    {
        // Arrange
        await SetupDatabase(59322, config =>
        {
            config.PlexServerCount = 1;
            config.PlexTvShowLibraryCount = 1;
            config.TvShowCount = 1;
            config.TvShowSeasonCount = 1;
            config.TvShowEpisodeCount = 0;
        });
        using var dbContext = IDbContext;
        var database = ((DbContext)dbContext).Database;
        var migrator = database.GetService<IMigrator>();
        await migrator.MigrateAsync("20261017134604_TEMP3", CancellationToken);
        var show = await dbContext.PlexTvShows.Select(x => new { x.Id, x.PlexLibraryId })
            .SingleAsync(CancellationToken);
        var seasonId = await dbContext.PlexTvShowSeason.Select(x => x.Id).SingleAsync(CancellationToken);
        (await dbContext.PlexTvShowEpisodes.ToListAsync(CancellationToken)).ShouldBeEmpty();
        await dbContext.PlexTvShows.ExecuteUpdateAsync(
            setters => setters.SetProperty(x => x.VideoQuality, VideoQuality.UHD_4K)
                .SetProperty(x => x.MediaSize, 100L).SetProperty(x => x.Duration, 60)
                .SetProperty(x => x.ChildCount, 4).SetProperty(x => x.GrandChildCount, 5), CancellationToken);
        await dbContext.PlexTvShowSeason.ExecuteUpdateAsync(
            setters => setters.SetProperty(x => x.MediaSize, 100L).SetProperty(x => x.Duration, 60)
                .SetProperty(x => x.ChildCount, 5), CancellationToken);
        await dbContext.PlexLibraries.ExecuteUpdateAsync(
            setters => setters.SetProperty(x => x.MediaSize, 100L), CancellationToken);
        dbContext.PlexTvShowMediaQualities.Add(new PlexTvShowMediaQuality
        {
            PlexTvShowId = show.Id, PlexLibraryId = show.PlexLibraryId, VideoQuality = VideoQuality.UHD_4K,
        });
        dbContext.PlexTvShowSeasonMediaQualities.Add(new PlexTvShowSeasonMediaQuality
        {
            PlexTvShowSeasonId = seasonId, PlexLibraryId = show.PlexLibraryId, VideoQuality = VideoQuality.UHD_4K,
        });
        await dbContext.SaveChangesAsync(CancellationToken);
        (await dbContext.PlexTvShows.Select(x => x.VideoQuality).SingleAsync(CancellationToken))
            .ShouldBe(VideoQuality.UHD_4K);
        (await dbContext.PlexTvShowMediaQualities.Select(x => x.PlexTvShowId)
            .ToListAsync(CancellationToken)).ShouldBe([show.Id]);
        (await dbContext.PlexTvShowSeasonMediaQualities.Select(x => x.PlexTvShowSeasonId)
            .ToListAsync(CancellationToken)).ShouldBe([seasonId]);

        // Act
        await migrator.MigrateAsync(cancellationToken: CancellationToken);
        dbContext.ClearChangeTracker();

        // Assert
        (await dbContext.PlexTvShows
            .Select(x => new { x.Id, x.VideoQuality, x.MediaSize, x.Duration, x.ChildCount, x.GrandChildCount })
            .SingleAsync(CancellationToken)).ShouldBe(new
            {
                show.Id, VideoQuality = VideoQuality.UHD_4K, MediaSize = 100L,
                Duration = 60, ChildCount = 4, GrandChildCount = 5,
            });
        (await dbContext.PlexTvShowSeason
            .Select(x => new { x.Id, x.MediaSize, x.Duration, x.ChildCount })
            .SingleAsync(CancellationToken)).ShouldBe(new
            {
                Id = seasonId, MediaSize = 100L, Duration = 60, ChildCount = 5,
            });
        (await dbContext.PlexTvShowMediaQualities.Select(x => x.PlexTvShowId)
            .ToListAsync(CancellationToken)).ShouldBe([show.Id]);
        (await dbContext.PlexTvShowSeasonMediaQualities.Select(x => x.PlexTvShowSeasonId)
            .ToListAsync(CancellationToken)).ShouldBe([seasonId]);
        (await dbContext.PlexLibraries.Select(x => x.MediaSize).SingleAsync(CancellationToken)).ShouldBe(100L);
    }

    [Test]
    public async Task ShouldCreateCanonicalFileSchema_WhenMigratingFreshDatabase()
    {
        // Arrange
        await SetupDatabase(59321);
        using var dbContext = IDbContext;
        var database = ((DbContext)dbContext).Database;
        await database.OpenConnectionAsync(CancellationToken);
        await using var command = database.GetDbConnection().CreateCommand();
        command.CommandText = """
            SELECT COUNT(*) FROM (
                SELECT name FROM pragma_table_info('PlexMovieData') WHERE name = 'VideoResolution'
                UNION ALL SELECT name FROM pragma_table_info('PlexTvShowEpisodeData') WHERE name = 'VideoResolution'
                UNION ALL SELECT name FROM pragma_table_info('PlexPhotoMediaData') WHERE name = 'VideoResolution'
                UNION ALL SELECT name FROM pragma_table_info('PlexOtherVideoData')
                    WHERE name IN ('VideoResolution', 'OriginalFilePath', 'SourceRelativePath')
                UNION ALL SELECT name FROM pragma_table_info('PlexMusicTrackData')
                    WHERE name IN ('OriginalFilePath', 'SourceRelativePath')
                UNION ALL SELECT name FROM sqlite_schema WHERE type = 'index' AND name IN (
                    'IX_PlexTvShowSeason_TvShowId',
                    'IX_PlexMusicArtistComparisons_RemotePlexLibraryId_OwnedPlexLibraryId',
                    'IX_PlexMusicAlbumComparisons_RemotePlexLibraryId_OwnedPlexLibraryId',
                    'IX_PlexMusicTrackComparisons_RemotePlexLibraryId_OwnedPlexLibraryId'
                )
            );
            """;

        // Act
        var obsoleteSchemaObjects = await command.ExecuteScalarAsync(CancellationToken);

        // Assert
        Convert.ToInt64(obsoleteSchemaObjects).ShouldBe(0);
        (await database.GetAppliedMigrationsAsync(CancellationToken)).Last()
            .ShouldBe("20261020003652_TEMP5");
    }

    [Test]
    public async Task ShouldCreateSingleFilePhotoSchema_WhenMigratingFreshDatabase()
    {
        // Arrange
        await SetupDatabase(59323);
        using var dbContext = IDbContext;
        var database = ((DbContext)dbContext).Database;
        await database.OpenConnectionAsync(CancellationToken);
        await using var command = database.GetDbConnection().CreateCommand();
        command.CommandText = """
            SELECT name FROM sqlite_schema
            WHERE type = 'table' AND (name LIKE 'PlexPhoto%' OR name LIKE 'DownloadTaskPhoto%')
            ORDER BY name;
            """;

        // Act
        var tables = new List<string>();
        await using (var reader = await command.ExecuteReaderAsync(CancellationToken))
        {
            while (await reader.ReadAsync(CancellationToken))
                tables.Add(reader.GetString(0));
        }
        command.CommandText = """
            SELECT 'catalog-folder:' || on_delete
                FROM pragma_foreign_key_list('PlexPhotoMediaData') WHERE "from" = 'PlexPhotoFolderId'
            UNION ALL SELECT 'membership-album:' || on_delete
                FROM pragma_foreign_key_list('PlexPhotoAlbumMemberships') WHERE "from" = 'PlexPhotoAlbumId'
            UNION ALL SELECT 'membership-file:' || on_delete
                FROM pragma_foreign_key_list('PlexPhotoAlbumMemberships') WHERE "from" = 'PlexPhotoMediaDataId'
            UNION ALL SELECT 'queue-folder:' || on_delete
                FROM pragma_foreign_key_list('DownloadTaskPhotoImageFiles') WHERE "from" = 'ParentId'
            UNION ALL SELECT 'queue-log:' || on_delete
                FROM pragma_foreign_key_list('DownloadTaskPhotoImageFileLogs') WHERE "from" = 'DownloadTaskFileId'
            ORDER BY 1;
            """;
        var deletionRules = new List<string>();
        await using (var reader = await command.ExecuteReaderAsync(CancellationToken))
        {
            while (await reader.ReadAsync(CancellationToken))
                deletionRules.Add(reader.GetString(0));
        }

        // Assert
        tables.ShouldBe([
            "DownloadTaskPhotoFolders",
            "DownloadTaskPhotoImageFileLogs",
            "DownloadTaskPhotoImageFiles",
            "PlexPhotoAlbumMemberships",
            "PlexPhotoAlbums",
            "PlexPhotoFolders",
            "PlexPhotoMediaData",
        ]);
        deletionRules.ShouldBe([
            "catalog-folder:SET NULL",
            "membership-album:CASCADE",
            "membership-file:CASCADE",
            "queue-folder:CASCADE",
            "queue-log:CASCADE",
        ]);
    }
}
