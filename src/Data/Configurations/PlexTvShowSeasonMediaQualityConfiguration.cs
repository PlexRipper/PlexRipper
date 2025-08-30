using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reaparr.Data.Configurations;

public class PlexTvShowSeasonMediaQualityConfiguration : IEntityTypeConfiguration<PlexTvShowSeasonMediaQuality>
{
    public void Configure(EntityTypeBuilder<PlexTvShowSeasonMediaQuality> builder)
    {
        builder
            .HasOne(e => e.PlexLibrary)
            .WithMany()
            .HasForeignKey(e => e.PlexLibraryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
