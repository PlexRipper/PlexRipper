namespace Reaparr.Data;

// ReSharper disable once InconsistentNaming
public static class ReaparrDBContextSeed
{
    private static ILogger _log = Log.ForContext(typeof(ReaparrDBContextSeed));

    public static List<FolderPath> GetDefaultFolderPaths()
    {
        // NOTE: Don't change the DirectoryPath to something dynamic, this will make the EF core migrations fail due to the seed data becoming inconsistent between migrations.
        var rootPath = PathProvider.DataDirectory;

        return
        [
            new FolderPath
            {
                Id = PlexMediaType.None.ToDefaultDestinationFolderId(),
                DisplayName = "Download Path",
                DirectoryPath = Path.Combine(rootPath, PathProvider.DefaultDownloadsFolderName),
                FolderType = FolderType.DownloadFolder,
                MediaType = PlexMediaType.None,
            },
            new FolderPath
            {
                Id = PlexMediaType.Movie.ToDefaultDestinationFolderId(),
                DisplayName = "Movie Destination Path",
                DirectoryPath = Path.Combine(rootPath, PathProvider.DefaultMovieFolderName),
                FolderType = FolderType.MovieFolder,
                MediaType = PlexMediaType.Movie,
            },
            new FolderPath
            {
                Id = PlexMediaType.TvShow.ToDefaultDestinationFolderId(),
                DisplayName = "Tv Show Destination Path",
                DirectoryPath = Path.Combine(rootPath, PathProvider.DefaultTvShowsFolderName),
                FolderType = FolderType.TvShowFolder,
                MediaType = PlexMediaType.TvShow,
            },
            new FolderPath
            {
                Id = PlexMediaType.Music.ToDefaultDestinationFolderId(),
                DisplayName = "Music Destination Path",
                DirectoryPath = Path.Combine(rootPath, PathProvider.DefaultMusicFolderName),
                FolderType = FolderType.MusicFolder,
                MediaType = PlexMediaType.Music,
            },
            new FolderPath
            {
                Id = PlexMediaType.Photos.ToDefaultDestinationFolderId(),
                DisplayName = "Photos Destination Path",
                DirectoryPath = Path.Combine(rootPath, PathProvider.DefaultPhotosFolderName),
                FolderType = FolderType.PhotosFolder,
                MediaType = PlexMediaType.Photos,
            },
            new FolderPath
            {
                Id = PlexMediaType.OtherVideos.ToDefaultDestinationFolderId(),
                DisplayName = "Other Videos Destination Path",
                DirectoryPath = Path.Combine(rootPath, PathProvider.DefaultOtherFolderName),
                FolderType = FolderType.OtherVideosFolder,
                MediaType = PlexMediaType.OtherVideos,
            },
            new FolderPath
            {
                Id = PlexMediaType.Games.ToDefaultDestinationFolderId(),
                DisplayName = "Games Videos Destination Path",
                DirectoryPath = Path.Combine(rootPath, PathProvider.DefaultGamesFolderName),
                FolderType = FolderType.GamesVideosFolder,
                MediaType = PlexMediaType.Games,
            },
            new FolderPath
            {
                Id = 8,
                DisplayName = "Reserved #1 Destination Path",
                DirectoryPath = rootPath,
                FolderType = FolderType.None,
                MediaType = PlexMediaType.None,
            },
            new FolderPath
            {
                Id = 9,
                DisplayName = "Reserved #2 Destination Path",
                DirectoryPath = rootPath,
                FolderType = FolderType.None,
                MediaType = PlexMediaType.None,
            },
            new FolderPath
            {
                Id = 10,
                DisplayName = "Reserved #3 Destination Path",
                DirectoryPath = rootPath,
                FolderType = FolderType.None,
                MediaType = PlexMediaType.None,
            },
        ];
    }

    public static Action<DbContext, bool> Seed() =>
        (context, _) =>
        {
            if (context is not ReaparrDbContext db)
                return;

            if (db.FolderPaths.Any())
                return;

            foreach (var path in GetDefaultFolderPaths())
            {
                _log.Here()
                    .Debug(
                        "Seeding default folder path with id {Id} and directory {DirectoryPath}",
                        path.Id,
                        path.DirectoryPath
                    );
                db.FolderPaths.Add(path);
            }

            db.SaveChanges();

            Verify(db);
        };

    public static Func<DbContext, bool, CancellationToken, Task> SeedAsync() =>
        async (context, _, cancellationToken) =>
        {
            if (context is not ReaparrDbContext db)
                return;

            if (await db.FolderPaths.AnyAsync(cancellationToken))
                return;

            foreach (var path in GetDefaultFolderPaths())
            {
                _log.Here()
                    .Debug(
                        "Seeding default folder path with id {Id} and directory {DirectoryPath}",
                        path.Id,
                        path.DirectoryPath
                    );
                await db.FolderPaths.AddAsync(path, cancellationToken);
            }

            await db.SaveChangesAsync(cancellationToken);

            Verify(db);
        };

    private static void Verify(ReaparrDbContext db)
    {
        var verify = db.FolderPaths.AsNoTracking().ToList();

        foreach (var v in verify)
        {
            _log.Here().Debug("DB AFTER SAVE: {Id} => {Path}", v.Id, v.DirectoryPath);
        }
    }
}
