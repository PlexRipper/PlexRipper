using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlexRipper.Domain;

namespace PlexRipper.Data.Configurations
{
    public class PlexAccountConfiguration : IEntityTypeConfiguration<PlexAccount>
    {
        public void Configure(EntityTypeBuilder<PlexAccount> builder)
        {
            builder.HasMany(x => x.PlexAccountServers).WithOne(x => x.PlexAccount).OnDelete(DeleteBehavior.Cascade);
        }
    }
}