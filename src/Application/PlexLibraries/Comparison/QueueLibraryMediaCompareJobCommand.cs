using FastEndpoints;
using FluentResults;

namespace Reaparr.BackgroundJobs.Contracts;

/// <summary>
/// Enqueues a library comparison job for a specific remote→owned library pair and media type.
/// Dispatched after library sync completes, ownership changes, or access changes.
/// </summary>
public record QueueLibraryMediaCompareJobCommand(
    int RemotePlexLibraryId,
    int OwnedPlexLibraryId,
    PlexMediaType MediaType
) : ICommand<Result>;
