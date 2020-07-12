using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlexRipper.Domain.Entities;

namespace PlexRipper.Data.Configurations
{
    public class PlexAccountConfiguration : IEntityTypeConfiguration<PlexAccount>
    {
        public void Configure(EntityTypeBuilder<PlexAccount> builder)
        {

        }
    }
}
