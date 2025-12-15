using Reaparr.Domain;

namespace Reaparr.BackgroundJobs.Contracts;

public record LibrarySyncJobQueueDTO
{
    /// <summary>
    /// Priority of the sync job.
    /// Lower values indicate higher priority.
    /// </summary>
    public required int Priority { get; init; }

    /// <summary>
    /// Current status of the sync job.
    /// </summary>
    public required LibrarySyncJobStatus Status { get; init; }

    /// <summary>
    /// UTC timestamp when the job was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// UTC timestamp when processing of the job started.
    /// Null if the job has not started yet.
    /// </summary>
    public required DateTime? StartedAt { get; init; }

    /// <summary>
    /// UTC timestamp when processing of the job completed.
    /// Null if the job has not completed yet.
    /// </summary>
    public required DateTime? CompletedAt { get; init; }

    /// <summary>
    /// Error message describing the failure, if the job did not complete successfully.
    /// </summary>
    public required string? ErrorMessage { get; init; }

    /// <summary>
    /// Identifier of the Plex library associated with this sync job.
    /// </summary>
    public required int PlexLibraryId { get; init; }

    /// <summary>
    /// Identifier of the Plex server associated with this sync job.
    /// </summary>
    public required int PlexServerId { get; init; }
}
