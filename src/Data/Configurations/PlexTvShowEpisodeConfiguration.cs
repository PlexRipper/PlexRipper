using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlexRipper.Domain;

namespace PlexRipper.Data.Configurations
{
    public class PlexTvShowGenreConfiguration : IEntityTypeConfiguration<PlexTvShowGenre>
    {
        public void Configure(EntityTypeBuilder<PlexTvShowGenre> builder)
        {
            builder
                .HasKey(bc => new { bc.PlexTvShowId, bc.PlexGenreId });
        }
    }
}