using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexMovieMediaQualityConfiguration : IEntityTypeConfiguration<PlexMovieMediaQuality>
{
    public void Configure(EntityTypeBuilder<PlexMovieMediaQuality> builder)
    {
        builder.HasKey(bc => new
        {
            bc.PlexMediaQualityId,
            bc.PlexLibraryId,
            bc.PlexMovieId,
        });

        // Foreign key to PlexMediaQuality
        builder
            .HasOne(x => x.PlexMediaQuality)
            .WithMany()
            .HasForeignKey(x => x.PlexMediaQualityId)
            .OnDelete(DeleteBehavior.Cascade);

        // Foreign key to PlexLibrary
        builder
            .HasOne(x => x.PlexLibrary)
            .WithMany()
            .HasForeignKey(x => x.PlexLibraryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Foreign key to PlexMovie
        builder.HasOne(x => x.PlexMovie).WithMany().HasForeignKey(x => x.PlexMovieId).OnDelete(DeleteBehavior.Cascade);

        // Foreign key to PlexMovieMediaData
        builder
            .HasOne(x => x.PlexMovieMediaData)
            .WithMany()
            .HasForeignKey(x => x.PlexMovieMediaDataId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
