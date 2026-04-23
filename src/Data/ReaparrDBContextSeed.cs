namespace Reaparr.Data;

// ReSharper disable once InconsistentNaming
public static class ReaparrDBContextSeed
{
    private static readonly ILogger _log = Log.ForContext(typeof(ReaparrDBContextSeed));

    public static List<FolderPath> GetDefaultFolderPaths() =>
        [
            new()
            {
                Id = PlexMediaType.None.ToDefaultDestinationFolderId(),
                DisplayName = "Download Path",
                DirectoryPath = PathProvider.DefaultDownloadsDestinationFolder,
                FolderType = FolderType.DownloadFolder,
                MediaType = PlexMediaType.None,
            },
            new()
            {
                Id = PlexMediaType.Movie.ToDefaultDestinationFolderId(),
                DisplayName = "Movie Destination Path",
                DirectoryPath = PathProvider.DefaultMovieDestinationFolder,
                FolderType = FolderType.MovieFolder,
                MediaType = PlexMediaType.Movie,
            },
            new()
            {
                Id = PlexMediaType.TvShow.ToDefaultDestinationFolderId(),
                DisplayName = "Tv Show Destination Path",
                DirectoryPath = PathProvider.DefaultTvShowsDestinationFolder,
                FolderType = FolderType.TvShowFolder,
                MediaType = PlexMediaType.TvShow,
            },
            new()
            {
                Id = PlexMediaType.Music.ToDefaultDestinationFolderId(),
                DisplayName = "Music Destination Path",
                DirectoryPath = PathProvider.DefaultMusicDestinationFolder,
                FolderType = FolderType.MusicFolder,
                MediaType = PlexMediaType.Music,
            },
            new()
            {
                Id = PlexMediaType.Photos.ToDefaultDestinationFolderId(),
                DisplayName = "Photos Destination Path",
                DirectoryPath = PathProvider.DefaultPhotosDestinationFolder,
                FolderType = FolderType.PhotosFolder,
                MediaType = PlexMediaType.Photos,
            },
            new()
            {
                Id = PlexMediaType.OtherVideos.ToDefaultDestinationFolderId(),
                DisplayName = "Other Videos Destination Path",
                DirectoryPath = PathProvider.DefaultOtherDestinationFolder,
                FolderType = FolderType.OtherVideosFolder,
                MediaType = PlexMediaType.OtherVideos,
            },
            new()
            {
                Id = PlexMediaType.Games.ToDefaultDestinationFolderId(),
                DisplayName = "Games Videos Destination Path",
                DirectoryPath = PathProvider.DefaultGamesDestinationFolder,
                FolderType = FolderType.GamesVideosFolder,
                MediaType = PlexMediaType.Games,
            },
            new()
            {
                Id = 8,
                DisplayName = "Reserved #1 Destination Path",
                DirectoryPath = PathProvider.DataDirectory,
                FolderType = FolderType.None,
                MediaType = PlexMediaType.None,
            },
            new()
            {
                Id = 9,
                DisplayName = "Reserved #2 Destination Path",
                DirectoryPath = PathProvider.DataDirectory,
                FolderType = FolderType.None,
                MediaType = PlexMediaType.None,
            },
            new()
            {
                Id = 10,
                DisplayName = "Reserved #3 Destination Path",
                DirectoryPath = PathProvider.DataDirectory,
                FolderType = FolderType.None,
                MediaType = PlexMediaType.None,
            },
        ];

    public static Action<DbContext, bool> Seed() =>
        (context, _) =>
        {
            if (context is not ReaparrDbContext db)
                return;

            var existingFolderPathIds = db.FolderPaths.Select(x => x.Id).ToHashSet();

            foreach (var path in GetDefaultFolderPaths().Where(x => !existingFolderPathIds.Contains(x.Id)))
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

            var existingFolderPathIds = await db.FolderPaths.Select(x => x.Id).ToHashSetAsync(cancellationToken);

            foreach (var path in GetDefaultFolderPaths().Where(x => !existingFolderPathIds.Contains(x.Id)))
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
