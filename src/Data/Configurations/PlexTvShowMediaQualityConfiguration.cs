using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexTvShowMediaQualityConfiguration : IEntityTypeConfiguration<PlexTvShowMediaQuality>
{
    public void Configure(EntityTypeBuilder<PlexTvShowMediaQuality> builder)
    {
        builder.HasKey(bc => new
        {
            bc.PlexMediaQualityId,
            bc.PlexLibraryId,
            bc.PlexTvShowId,
        });

        builder
            .HasOne(x => x.PlexMediaQuality)
            .WithMany()
            .HasForeignKey(x => x.PlexMediaQualityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.PlexLibrary)
            .WithMany()
            .HasForeignKey(x => x.PlexLibraryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.PlexTvShow)
            .WithMany()
            .HasForeignKey(x => x.PlexTvShowId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
