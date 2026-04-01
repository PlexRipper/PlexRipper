namespace Reaparr.Data.Configurations;

public class PlexGenreConfiguration : IEntityTypeConfiguration<PlexGenre>
{
    public void Configure(EntityTypeBuilder<PlexGenre> builder)
    {
        builder.HasIndex(x => x.Key).IsUnique();
    }
}
