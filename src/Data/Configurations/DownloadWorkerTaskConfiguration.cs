using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reaparr.Data.Configurations;

public class DownloadWorkerTaskConfiguration : IEntityTypeConfiguration<DownloadWorkerTask>
{
    public void Configure(EntityTypeBuilder<DownloadWorkerTask> builder)
    {
        builder
            .Property(b => b.DownloadStatus)
            .HasMaxLength(20)
            .HasConversion(x => x.ToDownloadStatusString(), x => x.ToDownloadStatus())
            .IsUnicode(false);
    }
}
