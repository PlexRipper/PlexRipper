using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class BasePlexMediaDataPartConfiguration : IEntityTypeConfiguration<BasePlexMediaDataPart>
{
    public void Configure(EntityTypeBuilder<BasePlexMediaDataPart> builder)
    {
        builder.UseTpcMappingStrategy();
    }
}
