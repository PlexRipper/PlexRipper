using System.Diagnostics.CodeAnalysis;
using FastEndpoints;
using FluentResults;

namespace Reaparr.BackgroundJobs.Contracts;

public record QueueNextPlexLibraryToSyncCommand : ICommand<Result>
{
    [SetsRequiredMembers]
    public QueueNextPlexLibraryToSyncCommand(int plexServerId, bool forceSync = false)
    {
        PlexServerId = plexServerId;
        ForceSync = forceSync;
    }

    public required int PlexServerId { get; init; }

    public required bool ForceSync { get; init; }
}
