using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexTvShowEpisodeMediaQualityConfiguration : IEntityTypeConfiguration<PlexTvShowEpisodeMediaQuality>
{
    public void Configure(EntityTypeBuilder<PlexTvShowEpisodeMediaQuality> builder)
    {
        builder.HasKey(bc => new
        {
            bc.PlexMediaQualityId,
            bc.PlexLibraryId,
            bc.PlexTvShowEpisodeId,
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

        // Foreign key to PlexTvShowEpisode
        builder
            .HasOne(x => x.PlexTvShowEpisode)
            .WithMany()
            .HasForeignKey(x => x.PlexTvShowEpisodeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Foreign key to PlexTvShowEpisodeMediaData
        builder
            .HasOne(x => x.PlexTvShowEpisodeMediaData)
            .WithMany()
            .HasForeignKey(x => x.PlexTvShowEpisodeMediaDataId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
