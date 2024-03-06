using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class DownloadTaskBaseConfiguration : IEntityTypeConfiguration<DownloadTaskBase>
{
    public void Configure(EntityTypeBuilder<DownloadTaskBase> builder)
    {
        builder.UseTpcMappingStrategy();
    }
}