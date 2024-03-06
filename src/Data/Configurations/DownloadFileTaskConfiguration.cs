using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class DownloadFileTaskConfiguration : IEntityTypeConfiguration<FileTask>
{
    public void Configure(EntityTypeBuilder<FileTask> builder) { }
}