using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.Configurations
{
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
                .Property(b => b.MediaType)
                .HasMaxLength(20)
                .HasConversion(x => x.ToPlexMediaTypeString(), x => x.ToPlexMediaType())
                .IsUnicode(false);

            builder
                .Property(b => b.DownloadStatus)
                .HasMaxLength(20)
                .HasConversion(x => x.ToDownloadStatusString(), x => x.ToDownloadStatus())
                .IsUnicode(false);

            builder
                .Property(x => x.MetaData)
                .HasJsonValueConversion();
        }
    }
}