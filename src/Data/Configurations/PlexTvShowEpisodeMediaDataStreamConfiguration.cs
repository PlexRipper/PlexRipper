using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexTvShowEpisodeMediaDataStreamConfiguration : IEntityTypeConfiguration<PlexTvShowEpisodeMediaDataStream>
{
    public void Configure(EntityTypeBuilder<PlexTvShowEpisodeMediaDataStream> builder)
    {
        builder.UseTpcMappingStrategy();

        // Configure relationship with PlexTvShowEpisode
        builder
            .HasOne(x => x.PlexTvShowEpisode)
            .WithMany()
            .HasForeignKey(x => x.PlexTvShowEpisodeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure relationship with PlexTvShowEpisodeMediaData
        builder
            .HasOne(x => x.PlexTvShowEpisodeMediaData)
            .WithMany()
            .HasForeignKey(x => x.PlexTvShowEpisodeMediaDataId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure relationship with PlexTvShowEpisodeMediaDataPart
        builder
            .HasOne(x => x.PlexTvShowEpisodeMediaDataPart)
            .WithMany(x => x.Streams)
            .HasForeignKey(x => x.PlexTvShowEpisodeMediaDataPartId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
