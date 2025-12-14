namespace Reaparr.Domain;

/// <summary>
/// Represents a queue item for syncing a Plex library.
/// </summary>
[Table("BackgroundJobLibrarySyncJobQueues")]
public class LibrarySyncJobQueue
{
    #region Properties

    /// <summary>
    /// Gets or sets the priority of this queue item. Lower numbers indicate higher priority.
    /// Movies typically have higher priority (lower number) than TV shows.
    /// </summary>
    [Column(Order = 1)]
    public required int Priority { get; set; }

    /// <summary>
    /// Gets or sets the status of this queue item.
    /// </summary>
    [Column(Order = 2)]
    public required LibrarySyncJobStatus Status { get; set; } = LibrarySyncJobStatus.Queued;

    /// <summary>
    /// Gets or sets when this queue item was created.
    /// </summary>
    [Column(Order = 3)]
    public required DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when processing of this queue item started.
    /// </summary>
    [Column(Order = 4)]
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Gets or sets when processing of this queue item completed (successfully or with error).
    /// </summary>
    [Column(Order = 5)]
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the error message if the sync failed.
    /// </summary>
    [Column(Order = 6)]
    public string? ErrorMessage { get; set; }

    #endregion

    #region Relationships

    /// <summary>
    /// Gets or sets the PlexLibrary this queue item is for.
    /// </summary>
    public PlexLibrary? PlexLibrary { get; set; }

    /// <summary>
    /// Gets or sets the PlexLibraryId this queue item is for.
    /// Part of the composite primary key along with PlexServerId.
    /// </summary>
    [Column(Order = 7)]
    public required int PlexLibraryId { get; set; }

    /// <summary>
    /// Gets or sets the PlexServer this queue item belongs to.
    /// </summary>
    public PlexServer? PlexServer { get; set; }

    /// <summary>
    /// Gets or sets the PlexServerId this queue item belongs to.
    /// Part of the composite primary key along with PlexLibraryId.
    /// </summary>
    [Column(Order = 8)]
    public required int PlexServerId { get; set; }

    #endregion
}
