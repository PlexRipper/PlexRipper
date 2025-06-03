using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexMovieConfiguration : IEntityTypeConfiguration<PlexMovie>
{
    public void Configure(EntityTypeBuilder<PlexMovie> builder)
    {
        builder.HasIndex(x => x.SortIndex);

        builder
            .HasMany(x => x.Roles)
            .WithMany(x => x.PlexMovieRoles)
            .UsingEntity<PlexMovieActors>(
                l => l.HasOne<PlexActor>().WithMany().HasForeignKey(e => e.PlexActorId),
                r => r.HasOne<PlexMovie>().WithMany().HasForeignKey(e => e.PlexMovieId)
            );

        builder
            .HasMany(x => x.Genres)
            .WithMany(x => x.PlexMovieGenres)
            .UsingEntity<PlexMovieGenres>(
                l => l.HasOne<PlexGenre>().WithMany().HasForeignKey(e => e.GenresId),
                r => r.HasOne<PlexMovie>().WithMany().HasForeignKey(e => e.PlexMovieId)
            );

        builder
            .HasMany(x => x.Countries)
            .WithMany(x => x.PlexMovieCountries)
            .UsingEntity<PlexMovieCountries>(
                l => l.HasOne<PlexCountry>().WithMany().HasForeignKey(e => e.CountryId),
                r => r.HasOne<PlexMovie>().WithMany().HasForeignKey(e => e.PlexMovieId)
            );

        builder
            .HasMany(x => x.MediaDataList)
            .WithOne(x => x.PlexMovie)
            .HasForeignKey(x => x.PlexMovieId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
