using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class DownloadTaskMovieFileConfiguration : IEntityTypeConfiguration<DownloadTaskMovieFile>
{
    public void Configure(EntityTypeBuilder<DownloadTaskMovieFile> builder)
    {
        builder
            .HasMany(x => x.DownloadWorkerTasks)
            .WithOne()
            .HasForeignKey(x => x.DownloadTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Property(b => b.DownloadStatus)
            .HasMaxLength(20)
            .HasConversion(x => x.ToDownloadStatusString(), x => x.ToDownloadStatus())
            .IsUnicode(false);

        builder.Property(c => c.FileName)
            .UseCollation(OrderByNaturalExtensions.CollationName);
    }
}