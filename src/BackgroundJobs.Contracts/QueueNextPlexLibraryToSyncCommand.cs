using System.Diagnostics.CodeAnalysis;
using FastEndpoints;
using FluentResults;

namespace Reaparr.BackgroundJobs.Contracts;

public record QueueNextPlexLibraryToSyncCommand : ICommand<Result>
{
    [SetsRequiredMembers]
    public QueueNextPlexLibraryToSyncCommand(int plexServerId)
    {
        PlexServerId = plexServerId;
    }

    public required int PlexServerId { get; init; }
}
