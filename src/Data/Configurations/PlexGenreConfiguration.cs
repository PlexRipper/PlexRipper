using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexGenreConfiguration : IEntityTypeConfiguration<PlexGenre>
{
    public void Configure(EntityTypeBuilder<PlexGenre> builder)
    {
        builder.HasIndex(x => x.PlexKey).IsUnique();
    }
}
