using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reaparr.Data.Configurations;

public class PlexTvShowMediaQualityConfiguration : IEntityTypeConfiguration<PlexTvShowMediaQuality>
{
    public void Configure(EntityTypeBuilder<PlexTvShowMediaQuality> builder)
    {
        builder
            .HasOne(e => e.PlexLibrary)
            .WithMany()
            .HasForeignKey(e => e.PlexLibraryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
