using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class DownloadTaskConfiguration : IEntityTypeConfiguration<DownloadTask>
{
    public void Configure(EntityTypeBuilder<DownloadTask> builder)
    {
        builder
            .HasMany(x => x.DownloadWorkerTasks)
            .WithOne(x => x.DownloadTask)
            .HasForeignKey(x => x.DownloadTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.Children)
            .WithOne(x => x.Parent)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Property(e => e.DownloadTaskType)
            .HasMaxLength(50)
            .HasConversion(x => x.ToDownloadTaskString(), x => x.ToDownloadTaskType())
            .IsUnicode(false);

        builder
            .Property(b => b.MediaType)
            .HasMaxLength(20)
            .HasConversion(x => x.ToPlexMediaTypeString(), x => x.ToPlexMediaType())
            .IsUnicode(false);

        builder
            .Property(b => b.DownloadStatus)
            .HasMaxLength(20)
            .HasConversion(x => x.ToDownloadStatusString(), x => x.ToDownloadStatus())
            .IsUnicode(false);

        builder.Property(c => c.Title)
            .UseCollation(OrderByNaturalExtensions.CollationName);
    }
}