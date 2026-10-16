namespace Reaparr.Data;

public class DownloadTaskParentBaseConfiguration : IEntityTypeConfiguration<DownloadTaskParentBase>
{
    public void Configure(EntityTypeBuilder<DownloadTaskParentBase> builder)
    {
        builder.UseTpcMappingStrategy();
    }
}
