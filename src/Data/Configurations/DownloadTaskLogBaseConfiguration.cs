using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reaparr.Data.Configurations;

public class DownloadTaskLogBaseConfiguration : IEntityTypeConfiguration<DownloadTaskLogBase>
{
    public void Configure(EntityTypeBuilder<DownloadTaskLogBase> builder)
    {
        builder.UseTpcMappingStrategy();

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
    }
}
