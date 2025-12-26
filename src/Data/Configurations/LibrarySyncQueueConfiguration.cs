using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reaparr.Data.Configurations;

public class LibrarySyncQueueConfiguration : IEntityTypeConfiguration<LibrarySyncJobQueue>
{
    public void Configure(EntityTypeBuilder<LibrarySyncJobQueue> builder)
    {
        // Configure a composite primary key
        builder.HasKey(x => new { x.PlexServerId, x.PlexLibraryId });

        builder
            .Property(e => e.Status)
            .HasMaxLength(50)
            .HasConversion(x => x.ToString(), x => Enum.Parse<LibrarySyncJobStatus>(x, true))
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
