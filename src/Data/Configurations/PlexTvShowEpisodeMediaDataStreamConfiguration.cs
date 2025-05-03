using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexTvShowEpisodeMediaDataStreamConfiguration : IEntityTypeConfiguration<PlexTvShowEpisodeMediaDataStream>
{
    public void Configure(EntityTypeBuilder<PlexTvShowEpisodeMediaDataStream> builder)
    {
        builder
            .HasOne(x => x.PlexTvShowEpisode)
            .WithMany()
            .HasForeignKey(x => x.PlexTvShowEpisodeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.PlexTvShowEpisodeMediaData)
            .WithMany()
            .HasForeignKey(x => x.PlexTvShowEpisodeMediaDataId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.PlexTvShowEpisodeMediaDataPart)
            .WithMany(x => x.Streams)
            .HasForeignKey(x => x.PlexTvShowEpisodeMediaDataPartId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
