using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class DownloadFileTaskConfiguration : IEntityTypeConfiguration<FileTask>
{
    public void Configure(EntityTypeBuilder<FileTask> builder)
    {
        builder
            .Property(e => e.DownloadTaskType)
            .HasMaxLength(50)
            .HasConversion(x => x.ToDownloadTaskString(), x => x.ToDownloadTaskType())
            .IsUnicode(false);
    }
}
