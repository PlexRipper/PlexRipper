using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations
{
    public class DownloadWorkerLogConfiguration : IEntityTypeConfiguration<DownloadWorkerLog>
    {
        public void Configure(EntityTypeBuilder<DownloadWorkerLog> builder)
        {
            builder
                .Property(b => b.LogLevel)
                .HasMaxLength(20)
                .HasConversion(x => x.ToNotificationLevelString(), x => x.ToNotificationLevel())
                .IsUnicode(false);
        }
    }
}