using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlexRipper.Domain.Entities.JoinTables;

namespace PlexRipper.Data.Configurations
{
    public class PlexTvShowGenreConfiguration : IEntityTypeConfiguration<PlexTvShowGenre>
    {
        public void Configure(EntityTypeBuilder<PlexTvShowGenre> builder)
        {
            builder
                .HasKey(bc => new { PlexTvShowId = bc.PlexTvShowId, bc.PlexGenreId });
        }
    }
}
