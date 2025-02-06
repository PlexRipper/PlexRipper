using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexMovieConfiguration : IEntityTypeConfiguration<PlexMovie>
{
    public void Configure(EntityTypeBuilder<PlexMovie> builder)
    {
        builder.HasIndex(x => x.SortIndex);

        builder.HasMany(x => x.Roles).WithMany(x => x.PlexMovieRoles).UsingEntity("PlexMovieRoles");

        builder.HasMany(x => x.Genres).WithMany(x => x.PlexMovieGenres).UsingEntity("PlexMovieGenres");

        builder.HasMany(x => x.Country).WithMany(x => x.PlexMovieCountries).UsingEntity("PlexMovieCountries");
    }
}
