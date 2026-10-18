namespace Reaparr.Data;

public class DownloadTaskBaseConfiguration : IEntityTypeConfiguration<DownloadTaskBase>
{
    public void Configure(EntityTypeBuilder<DownloadTaskBase> builder)
    {
        builder.UseTpcMappingStrategy();
    }
}
