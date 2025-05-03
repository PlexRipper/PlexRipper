using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class BasePlexMediaDataRowConfiguration : IEntityTypeConfiguration<BasePlexMediaDataRow>
{
    public void Configure(EntityTypeBuilder<BasePlexMediaDataRow> builder)
    {
        builder.UseTpcMappingStrategy();
    }
}
