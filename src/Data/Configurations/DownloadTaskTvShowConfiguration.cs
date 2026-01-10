using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reaparr.Data.Configurations;

public class DownloadTaskTvShowConfiguration : IEntityTypeConfiguration<DownloadTaskTvShow>
{
    public void Configure(EntityTypeBuilder<DownloadTaskTvShow> builder)
    {
        builder.HasIndex(x => x.DownloadStatus);
        builder.HasIndex(x => new { x.PlexServerId, x.PlexId });

        builder
            .HasMany(x => x.Children)
            .WithOne(x => x.Parent)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Property(b => b.DownloadStatus)
            .HasMaxLength(20)
            .HasConversion(x => x.ToDownloadStatusString(), x => x.ToDownloadStatus())
            .IsUnicode(false);

        builder.Property(c => c.Title).UseCollation(OrderByNaturalExtensions.CollationName);
    }
}
