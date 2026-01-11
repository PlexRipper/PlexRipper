using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reaparr.Data.Configurations;

public class DownloadTaskEpisodeFileConfiguration : IEntityTypeConfiguration<DownloadTaskTvShowEpisodeFile>
{
    public void Configure(EntityTypeBuilder<DownloadTaskTvShowEpisodeFile> builder)
    {
        builder.HasIndex(x => new
        {
            x.PlexLibraryId,
            x.PlexServerId,
            x.PlexId,
        });
        builder.HasIndex(x => x.HashId);

        builder
            .Property(b => b.DownloadStatus)
            .HasMaxLength(20)
            .HasConversion(x => x.ToDownloadStatusString(), x => x.ToDownloadStatus())
            .IsUnicode(false);

        builder.Property(c => c.FileName).UseCollation(OrderByNaturalExtensions.CollationName);
    }
}
