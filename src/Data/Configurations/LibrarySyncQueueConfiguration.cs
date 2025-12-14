using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reaparr.Domain;

namespace Reaparr.Data.Configurations;

public class LibrarySyncQueueConfiguration : IEntityTypeConfiguration<LibrarySyncQueue>
{
    public void Configure(EntityTypeBuilder<LibrarySyncQueue> builder)
    {
        builder
            .Property(e => e.Status)
            .HasMaxLength(50)
            .HasConversion(x => x.ToString(), x => Enum.Parse<LibrarySyncQueueStatus>(x))
            .IsUnicode(false);

        builder
            .HasOne(x => x.PlexLibrary)
            .WithMany()
            .HasForeignKey(x => x.PlexLibraryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.PlexServer)
            .WithMany()
            .HasForeignKey(x => x.PlexServerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
