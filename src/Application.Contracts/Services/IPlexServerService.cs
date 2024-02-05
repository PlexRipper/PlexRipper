using FluentResults;
using PlexRipper.Domain;

namespace Application.Contracts;

public interface IPlexServerService
{
    /// <summary>
    /// Will inspect all <see cref="PlexServer">PlexServers</see> added to this <see cref="PlexAccount"/>
    /// and checks its connectivity status and which libraries can be accessed.
    /// </summary>
    /// <param name="plexAccountId"></param>
    /// <param name="skipRefreshAccessibleServers"></param>
    /// <returns></returns>
    Task<Result> InspectAllPlexServersByAccountId(int plexAccountId, bool skipRefreshAccessibleServers = false);

    /// <summary>
    /// Refreshes the <see cref="PlexServer"/> data with connections from the Plex API
    /// </summary>
    /// <param name="plexServerId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<Result<PlexServer>> RefreshPlexServerConnectionsAsync(int plexServerId, CancellationToken token = default);

    Task<Result<PlexServer>> InspectPlexServer(int plexServerId);
}