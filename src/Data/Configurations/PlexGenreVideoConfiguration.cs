namespace Reaparr.Data.Configurations;

public class PlexGenreVideoConfiguration : IEntityTypeConfiguration<PlexGenreVideo>
{
    public void Configure(EntityTypeBuilder<PlexGenreVideo> builder)
    {
        builder.HasIndex(x => x.Key).IsUnique();
        builder
            .Property(x => x.Type)
            .HasJsonConversion()
            .HasMaxLength(20)
            .HasDefaultValue(PlexGenreType.Unknown)
            .IsUnicode(false);
    }
}
