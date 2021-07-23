using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlexRipper.Domain;

namespace PlexRipper.Data.Configurations
{
    public class FolderPathConfiguration : IEntityTypeConfiguration<FolderPath>
    {
        public void Configure(EntityTypeBuilder<FolderPath> builder)
        {
            builder
                .HasMany(x => x.PlexLibraries)
                .WithOne(x => x.DefaultDestination)
                .HasForeignKey(x => x.DefaultDestinationId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}