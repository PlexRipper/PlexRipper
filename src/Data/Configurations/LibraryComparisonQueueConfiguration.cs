namespace Reaparr.Data.Configurations;

/// <summary>
/// Configures the durable library comparison queue table used by the singleton comparison worker.
/// </summary>
public class LibraryComparisonQueueConfiguration : IEntityTypeConfiguration<LibraryComparisonJobQueue>
{
    public void Configure(EntityTypeBuilder<LibraryComparisonJobQueue> builder)
    {
        // One queue row per comparison scope keeps repeated invalidations idempotent.
        builder.HasKey(x => new
        {
            x.RemotePlexLibraryId,
            x.OwnedPlexLibraryId,
            x.MediaType,
        });

        builder
            .Property(e => e.Status)
            .HasMaxLength(50)
            .HasConversion(x => x.ToString(), x => Enum.Parse<LibrarySyncJobStatus>(x, true))
            .IsUnicode(false);

        builder
            .Property(e => e.MediaType)
            .HasMaxLength(50)
            .HasConversion(x => x.ToPlexMediaTypeString(), x => x.ToPlexMediaType())
            .IsUnicode(false);

        builder.HasIndex(x => new { x.Status, x.Priority, x.CreatedAt });

        builder
            .HasOne(x => x.RemotePlexLibrary)
            .WithMany()
            .HasForeignKey(x => x.RemotePlexLibraryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.OwnedPlexLibrary)
            .WithMany()
            .HasForeignKey(x => x.OwnedPlexLibraryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}