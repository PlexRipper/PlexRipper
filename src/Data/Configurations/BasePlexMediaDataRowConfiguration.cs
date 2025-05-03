using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class BasePlexMediaDataRowConfiguration : IEntityTypeConfiguration<BasePlexMediaData>
{
    public void Configure(EntityTypeBuilder<BasePlexMediaData> builder)
    {
        builder.UseTpcMappingStrategy();
    }
}
