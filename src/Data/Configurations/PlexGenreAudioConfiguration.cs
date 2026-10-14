namespace Reaparr.Data.Configurations;

public class PlexGenreAudioConfiguration : IEntityTypeConfiguration<PlexGenreAudio>
{
    public void Configure(EntityTypeBuilder<PlexGenreAudio> builder)
    {
        builder.HasIndex(x => x.Key).IsUnique();
    }
}
