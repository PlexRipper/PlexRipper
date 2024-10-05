using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class FolderPathConfiguration : IEntityTypeConfiguration<FolderPath>
{
    public void Configure(EntityTypeBuilder<FolderPath> builder)
    {
        builder
            .Property(b => b.FolderType)
            .HasMaxLength(50)
            .HasConversion(x => x.ToFolderTypeString(), x => x.ToFolderType())
            .IsUnicode(false);

        builder
            .Property(b => b.MediaType)
            .HasMaxLength(50)
            .HasConversion(x => x.ToPlexMediaTypeString(), x => x.ToPlexMediaType())
            .IsUnicode(false);

        builder
            .HasMany(x => x.PlexLibraries)
            .WithOne(x => x.DefaultDestination)
            .HasForeignKey(x => x.DefaultDestinationId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
