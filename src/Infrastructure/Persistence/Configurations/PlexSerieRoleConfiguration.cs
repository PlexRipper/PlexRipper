using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlexRipper.Domain.Entities.JoinTables;

namespace PlexRipper.Infrastructure.Persistence.Configurations
{
    public class PlexSerieRoleConfiguration : IEntityTypeConfiguration<PlexSerieRole>
    {
        public void Configure(EntityTypeBuilder<PlexSerieRole> builder)
        {
            builder
                .HasKey(bc => new { bc.PlexSerieId, bc.PlexGenreId });
        }
    }
}
