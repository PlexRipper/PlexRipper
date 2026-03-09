using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reaparr.Data.Configurations;

public class DownloadTaskFileBaseConfiguration : IEntityTypeConfiguration<DownloadTaskFileBase>
{
    public void Configure(EntityTypeBuilder<DownloadTaskFileBase> builder)
    {
        builder.UseTpcMappingStrategy();

        builder.HasIndex(x => x.DownloadStatus);

        // TODO:This can be removed once the EF Core issue is fixed: https://github.com/dotnet/efcore/issues/28443
        builder
            .Property(b => b.DirectoryMeta)
            .HasConversion(
                x => JsonSerializer.Serialize(x, DefaultJsonSerializerOptions.ConfigStandard),
                x => JsonSerializer.Deserialize<DownloadTaskDirectory>(x, DefaultJsonSerializerOptions.ConfigStandard)!
            )
            .IsUnicode();

        builder
            .Property(b => b.DirectDownloadSnapshot)
            .HasConversion(
                x => JsonSerializer.Serialize(x, DefaultJsonSerializerOptions.ConfigStandard),
                x => JsonSerializer.Deserialize<DirectDownloadSnapshot>(x, DefaultJsonSerializerOptions.ConfigStandard)!
            )
            .IsUnicode();

        builder
            .Property(b => b.DownloadClientType)
            .HasMaxLength(10)
            .HasConversion(x => x.ToPlexDownloadClientTypeString(), x => x.ToPlexDownloadClientType())
            .HasDefaultValue(PlexDownloadClientType.Direct)
            .IsUnicode(false);
    }
}
