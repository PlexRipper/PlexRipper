using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexTvShowConfiguration : IEntityTypeConfiguration<PlexTvShow>
{
    public void Configure(EntityTypeBuilder<PlexTvShow> builder)
    {
        builder.HasIndex(x => x.SortIndex);
    }
}
