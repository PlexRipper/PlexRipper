namespace Reaparr.Data;

// ReSharper disable once InconsistentNaming
public static class ReaparrDBContextSeed
{
    public static ModelBuilder SeedDatabase(ModelBuilder builder)
    {
        var list = new List<FolderPath>
        {
            new()
            {
                Id = PlexMediaType.None.ToDefaultDestinationFolderId(),
                DisplayName = "Download Path",
                DirectoryPath = PathProvider.DefaultDownloadsFolderName,
                FolderType = FolderType.DownloadFolder,
                MediaType = PlexMediaType.None,
            },
            new()
            {
                Id = PlexMediaType.Movie.ToDefaultDestinationFolderId(),
                DisplayName = "Movie Destination Path",
                DirectoryPath = PathProvider.DefaultMovieFolderName,
                FolderType = FolderType.MovieFolder,
                MediaType = PlexMediaType.Movie,
            },
            new()
            {
                Id = PlexMediaType.TvShow.ToDefaultDestinationFolderId(),
                DisplayName = "Tv Show Destination Path",
                DirectoryPath = PathProvider.DefaultTvShowsFolderName,
                FolderType = FolderType.TvShowFolder,
                MediaType = PlexMediaType.TvShow,
            },
            new()
            {
                Id = PlexMediaType.Music.ToDefaultDestinationFolderId(),
                DisplayName = "Music Destination Path",
                DirectoryPath = PathProvider.DefaultMusicFolderName,
                FolderType = FolderType.MusicFolder,
                MediaType = PlexMediaType.Music,
            },
            new()
            {
                Id = PlexMediaType.Photos.ToDefaultDestinationFolderId(),
                DisplayName = "Photos Destination Path",
                DirectoryPath = PathProvider.DefaultPhotosFolderName,
                FolderType = FolderType.PhotosFolder,
                MediaType = PlexMediaType.Photos,
            },
            new()
            {
                Id = PlexMediaType.OtherVideos.ToDefaultDestinationFolderId(),
                DisplayName = "Other Videos Destination Path",
                DirectoryPath = PathProvider.DefaultOtherFolderName,
                FolderType = FolderType.OtherVideosFolder,
                MediaType = PlexMediaType.OtherVideos,
            },
            new()
            {
                Id = PlexMediaType.Games.ToDefaultDestinationFolderId(),
                DisplayName = "Games Videos Destination Path",
                DirectoryPath = PathProvider.DefaultGamesFolderName,
                FolderType = FolderType.GamesVideosFolder,
                MediaType = PlexMediaType.Games,
            },
            new()
            {
                Id = 8,
                DisplayName = "Reserved #1 Destination Path",
                DirectoryPath = PathProvider.RootDirectory,
                FolderType = FolderType.None,
                MediaType = PlexMediaType.None,
            },
            new()
            {
                Id = 9,
                DisplayName = "Reserved #2 Destination Path",
                DirectoryPath = PathProvider.RootDirectory,
                FolderType = FolderType.None,
                MediaType = PlexMediaType.None,
            },
            new()
            {
                Id = 10,
                DisplayName = "Reserved #3 Destination Path",
                DirectoryPath = PathProvider.RootDirectory,
                FolderType = FolderType.None,
                MediaType = PlexMediaType.None,
            },
        };

        foreach (var folderPath in list)
            builder.Entity<FolderPath>().HasData(folderPath);

        return builder;
    }
}
