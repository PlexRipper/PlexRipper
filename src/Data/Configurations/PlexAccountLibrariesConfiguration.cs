using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlexRipper.Domain.Entities.JoinTables;

namespace PlexRipper.Data.Configurations
{
    public class PlexAccountLibrariesConfiguration : IEntityTypeConfiguration<PlexAccountLibrary>
    {
        public void Configure(EntityTypeBuilder<PlexAccountLibrary> builder)
        {
            builder
                .HasKey(bc => new { bc.PlexAccountId, bc.PlexLibraryId });
        }
    }
}
