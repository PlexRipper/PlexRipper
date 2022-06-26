using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations
{
    public class PlexMovieRoleConfiguration : IEntityTypeConfiguration<PlexMovieRole>
    {
        public void Configure(EntityTypeBuilder<PlexMovieRole> builder)
        {
            builder
                .HasKey(bc => new { bc.PlexMoviesId, bc.PlexGenreId });
        }
    }
}