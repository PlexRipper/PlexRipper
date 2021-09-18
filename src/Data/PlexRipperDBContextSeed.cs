using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace PlexRipper.Data
{
    public static class PlexRipperDBContextSeed
    {
        public static ModelBuilder SeedDatabase(ModelBuilder builder)
        {
            builder.Entity<FolderPath>().HasData(
                new FolderPath
                {
                    Id = 1,
                    DisplayName = "Download Path",
                    DirectoryPath = "/downloads",
                    FolderType = FolderType.DownloadFolder,
                    MediaType = PlexMediaType.None,
                });
            builder.Entity<FolderPath>().HasData(
                new FolderPath
                {
                    Id = 2,
                    DisplayName = "Movie Destination Path",
                    DirectoryPath = "/movies",
                    FolderType = FolderType.MovieFolder,
                    MediaType = PlexMediaType.Movie,
                });
            builder.Entity<FolderPath>().HasData(
                new FolderPath
                {
                    Id = 3,
                    DisplayName = "Tv Show Destination Path",
                    DirectoryPath = "/tvshows",
                    FolderType = FolderType.TvShowFolder,
                    MediaType = PlexMediaType.TvShow,
                });
            builder.Entity<FolderPath>().HasData(
                new FolderPath
                {
                    Id = 4,
                    DisplayName = "Music Destination Path",
                    DirectoryPath = "/music",
                    FolderType = FolderType.MusicFolder,
                    MediaType = PlexMediaType.Music,
                });
            builder.Entity<FolderPath>().HasData(
                new FolderPath
                {
                    Id = 5,
                    DisplayName = "Photos Destination Path",
                    DirectoryPath = "/photos",
                    FolderType = FolderType.PhotosFolder,
                    MediaType = PlexMediaType.Photos,
                });
            builder.Entity<FolderPath>().HasData(
                new FolderPath
                {
                    Id = 6,
                    DisplayName = "Other Videos Destination Path",
                    DirectoryPath = "/other",
                    FolderType = FolderType.OtherVideosFolder,
                    MediaType = PlexMediaType.OtherVideos,
                });
            builder.Entity<FolderPath>().HasData(
                new FolderPath
                {
                    Id = 7,
                    DisplayName = "Games Videos Destination Path",
                    DirectoryPath = "/games",
                    FolderType = FolderType.GamesVideosFolder,
                    MediaType = PlexMediaType.Games,
                });
            builder.Entity<FolderPath>().HasData(
                new FolderPath
                {
                    Id = 8,
                    DisplayName = "Reserved #1 Destination Path",
                    DirectoryPath = "/",
                    FolderType = FolderType.None,
                    MediaType = PlexMediaType.None,
                });
            builder.Entity<FolderPath>().HasData(
                new FolderPath
                {
                    Id = 9,
                    DisplayName = "Reserved #2 Destination Path",
                    DirectoryPath = "/",
                    FolderType = FolderType.None,
                    MediaType = PlexMediaType.None,
                });
            builder.Entity<FolderPath>().HasData(
                new FolderPath
                {
                    Id = 10,
                    DisplayName = "Reserved #3 Destination Path",
                    DirectoryPath = "/",
                    FolderType = FolderType.None,
                    MediaType = PlexMediaType.None,
                });

            return builder;
        }
    }
}