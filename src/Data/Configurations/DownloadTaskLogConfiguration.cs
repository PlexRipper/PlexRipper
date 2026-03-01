using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reaparr.Data.Configurations;

public class DownloadTaskLogConfiguration : IEntityTypeConfiguration<DownloadTaskLog>
{
    public void Configure(EntityTypeBuilder<DownloadTaskLog> builder)
    {
        builder
            .Property(b => b.LogLevel)
            .HasMaxLength(20)
            .HasConversion(x => x.ToNotificationLevelString(), x => x.ToNotificationLevel())
            .HasDefaultValue(NotificationLevel.None)
            .HasSentinel(NotificationLevel.None)
            .IsUnicode(false);

        builder
            .Property(b => b.Status)
            .HasMaxLength(20)
            .HasConversion(x => x.ToDownloadStatusString(), x => x.ToDownloadStatus())
            .HasDefaultValue(DownloadStatus.Unknown)
            .HasSentinel(DownloadStatus.Unknown)
            .IsUnicode(false);
    }
}
