using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlexRipper.Domain.Entities;

namespace PlexRipper.Data.Configurations
{
    public class PlexServerConfiguration : IEntityTypeConfiguration<PlexServer>
    {
        public void Configure(EntityTypeBuilder<PlexServer> builder)
        {
            builder
                .HasMany(x => x.PlexLibraries)
                .WithOne(x => x.PlexServer)
                .HasForeignKey(x => x.PlexServerId)
                .OnDelete(DeleteBehavior.Cascade);

            //builder
            //    .HasMany(x => x.ServerStatus)
            //    .WithOne(x => x.PlexServer)
            //    .HasForeignKey(x => x.PlexServerId)
            //    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
