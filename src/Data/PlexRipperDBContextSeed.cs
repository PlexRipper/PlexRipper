using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain.Entities;

namespace PlexRipper.Data
{
    public static class PlexRipperDBContextSeed
    {
        public static ModelBuilder SeedDatabase(ModelBuilder builder)
        {
            builder.Entity<FolderPath>().HasData(new FolderPath { Id = 1, DisplayName = "MoviePath", Directory = "/Movies" });
            builder.Entity<FolderPath>().HasData(new FolderPath { Id = 2, DisplayName = "SeriesPath", Directory = "/Series" });
            return builder;
        }
    }
}
