using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexTvShowEpisodeMediaDataConfiguration : IEntityTypeConfiguration<PlexTvShowEpisodeMediaData>
{
    public void Configure(EntityTypeBuilder<PlexTvShowEpisodeMediaData> builder)
    {
        builder.HasIndex(x => x.Quality);

        builder
            .HasMany(x => x.Parts)
            .WithOne(x => x.PlexTvShowEpisodeMediaData)
            .HasForeignKey(x => x.PlexTvShowEpisodeMediaDataId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.PlexTvShowEpisode)
            .WithMany(x => x.MediaDataList)
            .HasForeignKey(x => x.PlexTvShowEpisodeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
