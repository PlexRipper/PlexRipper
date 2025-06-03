using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexCountryConfiguration : IEntityTypeConfiguration<PlexCountry>
{
    public void Configure(EntityTypeBuilder<PlexCountry> builder)
    {
        builder.HasIndex(g => g.PlexKey).IsUnique();
    }
}
