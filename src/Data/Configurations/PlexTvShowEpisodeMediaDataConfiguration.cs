namespace Reaparr.Data;

public class PlexTvShowEpisodeMediaDataConfiguration : IEntityTypeConfiguration<PlexTvShowEpisodeMediaData>
{
    public void Configure(EntityTypeBuilder<PlexTvShowEpisodeMediaData> builder)
    {
        builder.HasIndex(x => x.VideoQuality);
        builder.HasIndex(x => new { x.PlexTvShowEpisodeId, Quality = x.VideoQuality });
        builder.HasIndex(x => x.PlexApiRatingKey);

        builder
            .HasOne(x => x.PlexTvShowEpisode)
            .WithMany(x => x.MediaDataList)
            .HasForeignKey(x => x.PlexTvShowEpisodeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
