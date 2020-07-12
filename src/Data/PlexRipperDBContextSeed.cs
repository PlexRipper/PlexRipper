using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Enums;

namespace PlexRipper.Data
{
    public static class PlexRipperDBContextSeed
    {
        public static ModelBuilder SeedDatabase(ModelBuilder builder)
        {
            builder.Entity<FolderPath>().HasData(
                new FolderPath { Id = 1, DisplayName = "DownloadPath", Directory = "/Downloads", FolderType = FolderType.DownloadFolder });
            builder.Entity<FolderPath>().HasData(
                new FolderPath { Id = 2, DisplayName = "MoviePath", Directory = "/Movies", FolderType = FolderType.MovieFolder });
            builder.Entity<FolderPath>().HasData(
                new FolderPath { Id = 3, DisplayName = "SeriesPath", Directory = "/Series", FolderType = FolderType.TvShowFolder });

            return builder;
        }
    }
}
