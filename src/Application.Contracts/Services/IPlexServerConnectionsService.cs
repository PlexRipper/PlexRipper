using FluentResults;
using PlexRipper.Domain;

namespace Application.Contracts;

public interface IPlexServerConnectionsService
{
    Task<Result<PlexServerStatus>> CheckPlexServerConnectionStatusAsync(PlexServerConnection plexServerConnectionId, bool trimEntries = true);

    /// <summary>
    /// Checks if the <see cref="PlexServerConnection"/> is connectable and log the status in the database.
    /// </summary>
    /// <param name="plexServerConnectionId">The <see cref="PlexServerConnection"/> to check for connectivity.</param>
    /// <param name="trimEntries">Delete entries which are older than a certain threshold.</param>
    /// <returns>The latest <see cref="PlexServerStatus"/>.</returns>
    Task<Result<PlexServerStatus>> CheckPlexServerConnectionStatusAsync(
        int plexServerConnectionId,
        bool trimEntries = true);

    /// <summary>
    /// Checks every <see cref="PlexServerConnection"/> in parallel of a <see cref="PlexServer"/> whether it connects or not
    /// and then stores that <see cref="PlexServerStatus"/> in the database.
    /// </summary>
    /// <param name="plexServerId">The id of the <see cref="PlexServer"/> to check the connections for.</param>
    /// <returns>Returns successful result if any connection connected.</returns>
    Task<Result> CheckAllConnectionsOfPlexServerAsync(int plexServerId);

    Task<Result<PlexServerConnection>> GetPlexServerConnectionAsync(int plexServerConnectionId);

    Task<Result<List<PlexServerConnection>>> GetAllPlexServerConnectionsAsync();
    Task<Result> CheckAllConnectionsOfPlexServersByAccountIdAsync(int plexAccountId);
}