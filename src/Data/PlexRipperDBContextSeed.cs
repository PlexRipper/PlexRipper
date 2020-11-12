using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace PlexRipper.Data
{
    public static class PlexRipperDBContextSeed
    {
        public static ModelBuilder SeedDatabase(ModelBuilder builder)
        {
            builder.Entity<FolderPath>().HasData(
                new FolderPath { Id = 1, DisplayName = "Download Path", DirectoryPath = "/Downloads", FolderType = FolderType.DownloadFolder });
            builder.Entity<FolderPath>().HasData(
                new FolderPath { Id = 2, DisplayName = "Movie Destination Path", DirectoryPath = "/Movies", FolderType = FolderType.MovieFolder });
            builder.Entity<FolderPath>().HasData(
                new FolderPath { Id = 3, DisplayName = "Tv Show Destination Path", DirectoryPath = "/Series", FolderType = FolderType.TvShowFolder });

            return builder;
        }
    }
}
