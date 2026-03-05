using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reaparr.Data.Configurations;

public class DownloadTaskLogConfiguration : IEntityTypeConfiguration<DownloadTaskMovieFileLog>
{
    public void Configure(EntityTypeBuilder<DownloadTaskMovieFileLog> builder)
    {
        builder
            .Property(x => x.LogLevel)
            .HasMaxLength(20)
            .HasConversion(x => x.ToNotificationLevelString(), x => x.ToNotificationLevel())
            .HasDefaultValue(NotificationLevel.None)
            .HasSentinel(NotificationLevel.None)
            .IsUnicode(false);

        builder
            .Property(x => x.Status)
            .HasMaxLength(20)
            .HasConversion(x => x.ToDownloadStatusString(), x => x.ToDownloadStatus())
            .HasDefaultValue(DownloadStatus.Unknown)
            .HasSentinel(DownloadStatus.Unknown)
            .IsUnicode(false);

        builder
            .HasOne(x => x.DownloadTaskFile)
            .WithMany(x => x.Logs)
            .HasForeignKey(b => b.DownloadTaskFileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
