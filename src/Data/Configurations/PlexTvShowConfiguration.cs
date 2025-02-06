using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexTvShowConfiguration : IEntityTypeConfiguration<PlexTvShow>
{
    public void Configure(EntityTypeBuilder<PlexTvShow> builder)
    {
        builder.HasIndex(x => x.SortIndex);

        builder.HasMany(x => x.Roles).WithMany(x => x.PlexTvShowRoles).UsingEntity("PlexTvShowRoles");

        builder.HasMany(x => x.Genres).WithMany(x => x.PlexTvShowGenres).UsingEntity("PlexTvShowGenres");

        builder.HasMany(x => x.Country).WithMany(x => x.PlexTvShowCountries).UsingEntity("PlexTvShowCountries");
    }
}
