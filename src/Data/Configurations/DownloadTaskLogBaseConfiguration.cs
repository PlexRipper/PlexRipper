namespace Reaparr.Data;

public class DownloadTaskLogBaseConfiguration : IEntityTypeConfiguration<DownloadTaskLogBase>
{
    public void Configure(EntityTypeBuilder<DownloadTaskLogBase> builder)
    {
        builder.UseTpcMappingStrategy();

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
    }
}
