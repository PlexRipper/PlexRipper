using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PlexRipper.Domain;

namespace PlexRipper.Data.Configurations
{
    public class DownloadWorkerTaskConfiguration : IEntityTypeConfiguration<DownloadWorkerTask>
    {
        public void Configure(EntityTypeBuilder<DownloadWorkerTask> builder)
        {
            builder
                .HasMany(x => x.DownloadWorkerTaskLogs)
                .WithOne(x => x.DownloadTask)
                .HasForeignKey(x => x.DownloadWorkerTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .Property(e => e.DownloadStatus)
                .HasConversion(new EnumToStringConverter<DownloadStatus>());
        }
    }
}