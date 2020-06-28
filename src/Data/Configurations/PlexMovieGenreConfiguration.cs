using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlexRipper.Domain.Entities.JoinTables;

namespace PlexRipper.Data.Configurations
{
    public class PlexMovieGenreConfiguration : IEntityTypeConfiguration<PlexMovieGenre>
    {
        public void Configure(EntityTypeBuilder<PlexMovieGenre> builder)
        {
            builder
                .HasKey(bc => new { bc.PlexMoviesId, bc.PlexGenreId });
        }
    }
}
