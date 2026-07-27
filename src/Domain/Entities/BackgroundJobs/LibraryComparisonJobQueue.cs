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
    /// <summary>
    /// Sort key for the queue worker; lower numeric values are processed first.
    /// </summary>
    [Column(Order = 1)]
    public required int Priority { get; init; }

    /// <summary>
    /// Current processing state of this queued comparison, such as Queued, Processing, Completed, or Failed.
    /// </summary>
    [Column(Order = 2)]
    public required LibrarySyncJobStatus Status { get; init; } = LibrarySyncJobStatus.Queued;

    /// <summary>
    /// UTC timestamp when this comparison work item was created or re-queued.
    /// </summary>
    [Column(Order = 3)]
    public required DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// UTC timestamp when the singleton comparison worker started processing this item.
    /// </summary>
    [Column(Order = 4)]
    public DateTime? StartedAt { get; init; }

    /// <summary>
    /// UTC timestamp when the worker completed or failed this item.
    /// </summary>
    [Column(Order = 5)]
    public DateTime? CompletedAt { get; init; }

    /// <summary>
    /// Number of processing attempts made for this comparison item.
    /// </summary>
    [Column(Order = 6)]
    public int Attempts { get; init; }

    /// <summary>
    /// Last failure message recorded for this item, if processing failed.
    /// </summary>
    [Column(Order = 7)]
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Media type to compare for the remote/owned library pair.
    /// </summary>
    public PlexMediaType MediaType { get; init; }

    /// <summary>
    /// Navigation to the remote library source.
    /// </summary>
    public PlexLibrary? RemotePlexLibrary { get; init; }

    /// <summary>
    /// Remote library source to compare from.
    /// </summary>
    [Column(Order = 8)]
    public required int RemotePlexLibraryId { get; init; }

    /// <summary>
    /// Navigation to the owned library target.
    /// </summary>
    public PlexLibrary? OwnedPlexLibrary { get; init; }

    /// <summary>
    /// Owned library target to compare against.
    /// </summary>
    [Column(Order = 9)]
    public required int OwnedPlexLibraryId { get; init; }
}