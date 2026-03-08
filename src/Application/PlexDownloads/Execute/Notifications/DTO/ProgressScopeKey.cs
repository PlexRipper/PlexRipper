namespace Reaparr.Application;

/// <summary>
/// Identifies a progress scope by Plex server and root task key.
/// </summary>
public sealed record ProgressScopeKey(int PlexServerId, DownloadTaskKey RootKey)
{
    /// <summary>
    /// Creates a normalized scope from a root key.
    /// </summary>
    public static ProgressScopeKey From(DownloadTaskKey rootKey) =>
        new(
            rootKey.PlexServerId,
            new DownloadTaskKey
            {
                Id = rootKey.Id,
                PlexServerId = rootKey.PlexServerId,
                PlexLibraryId = rootKey.PlexLibraryId,
                Type = rootKey.Type,
            }
        );
}
