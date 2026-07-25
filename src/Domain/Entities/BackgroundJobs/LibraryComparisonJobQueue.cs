namespace Reaparr.Domain;

/// <summary>
/// Persisted work item for comparing one remote Plex library against one owned Plex library.
/// </summary>
/// <remarks>
/// The composite key intentionally de-duplicates by remote library, owned library, and media type so repeated sync or
/// ownership invalidations refresh the same pending work instead of creating duplicate Quartz jobs.
/// Lower numeric <see cref="Priority"/> values run first; movies currently use 1 so they run before slower TV comparisons.
/// </remarks>
[Table("BackgroundJobLibraryComparisonJobQueues")]
public class LibraryComparisonJobQueue
{
    [Column(Order = 1)]
    public required int Priority { get; init; }

    [Column(Order = 2)]
    public required LibrarySyncJobStatus Status { get; init; } = LibrarySyncJobStatus.Queued;

    [Column(Order = 3)]
    public required DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    [Column(Order = 4)]
    public DateTime? StartedAt { get; init; }

    [Column(Order = 5)]
    public DateTime? CompletedAt { get; init; }

    [Column(Order = 6)]
    public int Attempts { get; init; }

    [Column(Order = 7)]
    public string? ErrorMessage { get; init; }

    public PlexMediaType MediaType { get; init; }

    public PlexLibrary? RemotePlexLibrary { get; init; }

    [Column(Order = 8)]
    public required int RemotePlexLibraryId { get; init; }

    public PlexLibrary? OwnedPlexLibrary { get; init; }

    [Column(Order = 9)]
    public required int OwnedPlexLibraryId { get; init; }
}
