using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlexRipper.Domain.Entities.JoinTables;

namespace PlexRipper.Data.Configurations
{
    public class PlexTvShowRoleConfiguration : IEntityTypeConfiguration<PlexTvShowRole>
    {
        public void Configure(EntityTypeBuilder<PlexTvShowRole> builder)
        {
            builder
                .HasKey(bc => new { PlexTvShowId = bc.PlexTvShowId, bc.PlexGenreId });
        }
    }
}
