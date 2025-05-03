using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class BasePlexMediaDataConfiguration : IEntityTypeConfiguration<BasePlexMediaData>
{
    public void Configure(EntityTypeBuilder<BasePlexMediaData> builder)
    {
        builder.UseTpcMappingStrategy();
    }
}
