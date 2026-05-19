namespace Reaparr.Data.Configurations;

public class PlexLibraryAccessHistoryEventConfiguration : IEntityTypeConfiguration<PlexLibraryAccessHistoryEvent>
{
    public void Configure(EntityTypeBuilder<PlexLibraryAccessHistoryEvent> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RefreshRunId).IsRequired();
        builder.Property(x => x.PlexAccountId).IsRequired();
        builder.Property(x => x.State).IsRequired().HasConversion<int>();
        builder.Property(x => x.OccurredAtUtc).IsRequired();

        builder.HasIndex(x => new { x.PlexAccountId, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.PlexAccountId, x.PlexServerId, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.PlexAccountId, x.PlexLibraryId, x.OccurredAtUtc });
        builder.HasIndex(x => x.RefreshRunId);
    }
}
