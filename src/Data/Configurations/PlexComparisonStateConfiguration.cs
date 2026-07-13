namespace Reaparr.Data.Configurations;

public class PlexComparisonStateConfiguration : IEntityTypeConfiguration<PlexComparisonState>
{
    public void Configure(EntityTypeBuilder<PlexComparisonState> builder)
    {
        builder
            .Property(e => e.MediaType)
            .HasMaxLength(50)
            .HasConversion(x => x.ToPlexMediaTypeString(), x => x.ToPlexMediaType())
            .IsUnicode(false);
    }
}
