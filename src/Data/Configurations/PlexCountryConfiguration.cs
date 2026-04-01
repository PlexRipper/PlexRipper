namespace Reaparr.Data.Configurations;

public class PlexCountryConfiguration : IEntityTypeConfiguration<PlexCountry>
{
    public void Configure(EntityTypeBuilder<PlexCountry> builder)
    {
        builder.HasIndex(x => x.Key).IsUnique();
    }
}
