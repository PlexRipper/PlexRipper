using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reaparr.Data.Configurations;

public class PlexTvShowEpisodeMediaDataConfiguration : IEntityTypeConfiguration<PlexTvShowEpisodeMediaData>
{
    public void Configure(EntityTypeBuilder<PlexTvShowEpisodeMediaData> builder)
    {
        builder.HasIndex(x => x.Quality);

        builder
            .HasOne(x => x.PlexTvShowEpisode)
            .WithMany(x => x.MediaDataList)
            .HasForeignKey(x => x.PlexTvShowEpisodeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
