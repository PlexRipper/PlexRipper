using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlexRipper.Domain.Config;

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

        // TODO This can be removed once the EF Core issue is fixed: https://github.com/dotnet/efcore/issues/28443
        builder
            .Property(b => b.DirectoryMeta)
            .HasConversion(
                x => JsonSerializer.Serialize(x, DefaultJsonSerializerOptions.ConfigStandard),
                x => JsonSerializer.Deserialize<DownloadTaskDirectory>(x, DefaultJsonSerializerOptions.ConfigStandard)!
            )
            .IsUnicode();
    }
}
