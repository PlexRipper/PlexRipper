namespace Reaparr.Data.Configurations;

public class PlexLibraryAccessHistoryEventConfiguration : IEntityTypeConfiguration<PlexLibraryAccessHistoryEvent>
{
    public void Configure(EntityTypeBuilder<PlexLibraryAccessHistoryEvent> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RefreshRunId).IsRequired();
        builder.Property(x => x.PlexAccountId).IsRequired();
        builder.Property(x => x.State).IsRequired().HasConversion<int>();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => new { x.PlexAccountId, OccurredAtUtc = x.CreatedAt });
        builder.HasIndex(x => new
        {
            x.PlexAccountId,
            x.PlexServerId,
            OccurredAtUtc = x.CreatedAt,
        });
        builder.HasIndex(x => new
        {
            x.PlexAccountId,
            x.PlexLibraryId,
            OccurredAtUtc = x.CreatedAt,
        });
        builder.HasIndex(x => x.RefreshRunId);
    }
}
