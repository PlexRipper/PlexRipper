namespace Reaparr.Data;

public class PlexOtherVideoDataStreamConfiguration : IEntityTypeConfiguration<PlexOtherVideoDataStream>
{
    public void Configure(EntityTypeBuilder<PlexOtherVideoDataStream> builder)
    {
        builder.HasBaseType((Type?)null);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.MediaDataId).IsRequired();
        builder.Property(x => x.PlexApiStreamId).IsRequired();
        builder.Property(x => x.StreamType).HasConversion<int>().IsRequired();
        builder.Property(x => x.LanguageId).IsRequired();

        builder.HasIndex(x => new { x.MediaDataId, x.PlexApiStreamId }).IsUnique();
        builder.HasIndex(x => new
        {
            x.LanguageId,
            x.StreamType,
            x.MediaDataId,
        });

        builder.HasOne(x => x.Language).WithMany().HasForeignKey(x => x.LanguageId).OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.PlexOtherVideoMediaData)
            .WithMany(x => x.Streams)
            .HasForeignKey(x => x.MediaDataId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
