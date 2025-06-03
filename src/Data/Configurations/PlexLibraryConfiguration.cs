using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexLibraryConfiguration : IEntityTypeConfiguration<PlexLibrary>
{
    public void Configure(EntityTypeBuilder<PlexLibrary> builder)
    {
        builder
            .Property(e => e.Type)
            .HasMaxLength(50)
            .HasConversion(x => x.ToPlexMediaTypeString(), x => x.ToPlexMediaType())
            .IsUnicode(false);

        builder
            .HasOne(x => x.PlexServer)
            .WithMany(x => x.PlexLibraries)
            .HasForeignKey(x => x.PlexServerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.Movies)
            .WithOne(x => x.PlexLibrary)
            .HasForeignKey(x => x.PlexLibraryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.TvShows)
            .WithOne(x => x.PlexLibrary)
            .HasForeignKey(x => x.PlexLibraryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.PlexAccountLibraries)
            .WithOne(x => x.PlexLibrary)
            .HasForeignKey(x => x.PlexLibraryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.DefaultDestination)
            .WithMany(x => x.PlexLibraries)
            .HasForeignKey(x => x.DefaultDestinationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(c => c.Title).UseCollation(OrderByNaturalExtensions.CollationName);

        builder
            .HasMany(x => x.Roles)
            .WithMany(x => x.PlexLibraries)
            .UsingEntity<PlexLibraryRoles>(
                l => l.HasOne<PlexActor>().WithMany().HasForeignKey(e => e.PlexRoleId),
                r => r.HasOne<PlexLibrary>().WithMany().HasForeignKey(e => e.PlexLibraryId)
            );

        builder
            .HasMany(x => x.Genres)
            .WithMany(x => x.PlexLibraries)
            .UsingEntity<PlexLibraryGenres>(
                l => l.HasOne<PlexGenre>().WithMany().HasForeignKey(e => e.PlexGenreId),
                r => r.HasOne<PlexLibrary>().WithMany().HasForeignKey(e => e.PlexLibraryId)
            );

        builder
            .HasMany(x => x.Countries)
            .WithMany(x => x.PlexLibraries)
            .UsingEntity<PlexLibraryCountries>(
                l => l.HasOne<PlexCountry>().WithMany().HasForeignKey(e => e.PlexCountryId),
                r => r.HasOne<PlexLibrary>().WithMany().HasForeignKey(e => e.PlexLibraryId)
            );
    }
}
