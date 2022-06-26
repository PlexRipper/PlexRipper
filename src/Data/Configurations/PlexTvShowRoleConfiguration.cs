using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations
{
    public class PlexTvShowRoleConfiguration : IEntityTypeConfiguration<PlexTvShowRole>
    {
        public void Configure(EntityTypeBuilder<PlexTvShowRole> builder)
        {
            builder
                .HasKey(bc => new { bc.PlexTvShowId, bc.PlexGenreId });
        }
    }
}