using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexTvShowEpisodeMediaDataPartConfiguration : IEntityTypeConfiguration<PlexTvShowEpisodeMediaDataPart>
{
    public void Configure(EntityTypeBuilder<PlexTvShowEpisodeMediaDataPart> builder)
    {
        builder.UseTpcMappingStrategy();

        builder
            .HasMany(x => x.Streams)
            .WithOne(x => x.PlexTvShowEpisodeMediaDataPart)
            .HasForeignKey(x => x.PlexTvShowEpisodeMediaDataPartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.PlexTvShowEpisode)
            .WithMany(x => x.Parts)
            .HasForeignKey(x => x.PlexTvShowEpisodeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.PlexTvShowEpisodeMediaData)
            .WithMany(x => x.Parts)
            .HasForeignKey(x => x.PlexTvShowEpisodeMediaDataId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
