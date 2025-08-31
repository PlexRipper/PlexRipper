using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reaparr.Data.Configurations;

public class PlexTvShowConfiguration : IEntityTypeConfiguration<PlexTvShow>
{
    public void Configure(EntityTypeBuilder<PlexTvShow> builder)
    {
        builder.HasIndex(x => x.SortIndex);

        builder
            .HasMany(x => x.Actors)
            .WithMany(x => x.PlexTvShowActors)
            .UsingEntity<PlexTvShowActors>(
                l => l.HasOne<PlexActor>().WithMany().HasForeignKey(e => e.PlexActorId),
                r => r.HasOne<PlexTvShow>().WithMany().HasForeignKey(e => e.PlexTvShowId)
            );

        builder
            .HasMany(x => x.Genres)
            .WithMany(x => x.PlexTvShowGenres)
            .UsingEntity<PlexTvShowGenres>(
                l => l.HasOne<PlexGenre>().WithMany().HasForeignKey(e => e.GenresId),
                r => r.HasOne<PlexTvShow>().WithMany().HasForeignKey(e => e.PlexTvShowId)
            );

        builder
            .HasMany(x => x.Countries)
            .WithMany(x => x.PlexTvShowCountries)
            .UsingEntity<PlexTvShowCountries>(
                l => l.HasOne<PlexCountry>().WithMany().HasForeignKey(e => e.CountryId),
                r => r.HasOne<PlexTvShow>().WithMany().HasForeignKey(e => e.PlexTvShowId)
            );

        builder
            .HasMany(x => x.Qualities)
            .WithOne(x => x.PlexTvShow)
            .HasForeignKey(x => x.PlexTvShowId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
