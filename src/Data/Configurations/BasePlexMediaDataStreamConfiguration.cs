using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class BasePlexMediaDataStreamConfiguration : IEntityTypeConfiguration<BasePlexMediaDataStream>
{
    public void Configure(EntityTypeBuilder<BasePlexMediaDataStream> builder)
    {
        builder.UseTpcMappingStrategy();
    }
}
