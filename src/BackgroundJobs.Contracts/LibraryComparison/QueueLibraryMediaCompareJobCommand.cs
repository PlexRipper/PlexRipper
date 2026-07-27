namespace Reaparr.BackgroundJobs.Contracts;

/// <summary>
/// Upserts one persisted comparison queue item for a remote-to-owned library pair and media type.
/// </summary>
/// <param name="RemotePlexLibraryId">The non-owned source library to compare from.</param>
/// <param name="OwnedPlexLibraryId">The owned target library to compare against.</param>
/// <param name="MediaType">The media family handled by the comparison worker.</param>
public record QueueLibraryMediaCompareJobCommand(
    int RemotePlexLibraryId,
    int OwnedPlexLibraryId,
    PlexMediaType MediaType
) : ICommand<Result>;