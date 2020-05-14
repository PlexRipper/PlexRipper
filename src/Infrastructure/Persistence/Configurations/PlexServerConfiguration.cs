

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlexRipper.Domain.Entities.Plex;

namespace PlexRipper.Infrastructure.Persistence.Configurations
{
    public class PlexServerConfiguration : IEntityTypeConfiguration<PlexServer>
    {
        public void Configure(EntityTypeBuilder<PlexServer> builder)
        {
            builder
                .HasMany(x => x.PlexLibraries)
                .WithOne(x => x.PlexServer)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
