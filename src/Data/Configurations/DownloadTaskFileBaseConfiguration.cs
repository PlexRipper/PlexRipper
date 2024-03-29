using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class DownloadTaskFileBaseConfiguration : IEntityTypeConfiguration<DownloadTaskFileBase>
{
    public void Configure(EntityTypeBuilder<DownloadTaskFileBase> builder)
    {
        builder.UseTpcMappingStrategy();

        builder
            .HasMany(x => x.DownloadWorkerTasks)
            .WithOne(x => x.DownloadTask)
            .HasForeignKey(x => x.DownloadTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(x => x.DirectoryMeta, cb => { cb.ToJson(); });
    }
}