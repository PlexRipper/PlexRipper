using FastEndpoints;
using FluentResults;

namespace Reaparr.BackgroundJobs.Contracts;

/// <summary>
/// Queues one or more library sync job for the specified Plex library IDs.
/// </summary>
public record QueueLibrarySyncJobCommand(List<int> PlexLibraryIds) : ICommand<Result>;
