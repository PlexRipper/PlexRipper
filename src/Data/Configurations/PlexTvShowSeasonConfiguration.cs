using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reaparr.Data.Configurations;

public class PlexTvShowSeasonConfiguration : IEntityTypeConfiguration<PlexTvShowSeason>
{
    public void Configure(EntityTypeBuilder<PlexTvShowSeason> builder)
    {
        builder.HasIndex(x => new { x.PlexLibraryId, x.SortIndex });

        builder.HasIndex(x => new { x.Key, x.PlexServerId });

        builder
            .HasMany(x => x.Qualities)
            .WithOne(x => x.PlexTvShowSeason)
            .HasForeignKey(x => x.PlexTvShowSeasonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
