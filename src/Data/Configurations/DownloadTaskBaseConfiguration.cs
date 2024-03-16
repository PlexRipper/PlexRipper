using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class DownloadTaskParentBaseConfiguration : IEntityTypeConfiguration<DownloadTaskParentBase>
{
    public void Configure(EntityTypeBuilder<DownloadTaskParentBase> builder)
    {
        builder.UseTpcMappingStrategy();
    }
}