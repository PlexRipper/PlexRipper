using FluentResults;
using PlexRipper.Domain;

namespace Application.Contracts;

public interface IPlexServerConnectionsService
{
    #region Methods

    #region Public

    Task<Result<PlexServerStatus>> CheckPlexServerConnectionStatusAsync(PlexServerConnection plexServerConnectionId, bool trimEntries = true);

    /// <summary>
    /// Checks every <see cref="PlexServerConnection"/> in parallel of a <see cref="PlexServer"/> whether it connects or not
    /// and then stores that <see cref="PlexServerStatus"/> in the database.
    /// </summary>
    /// <param name="plexServerId">The id of the <see cref="PlexServer" /> to check the connections for.</param>
    /// <returns>Returns successful result if any connection connected.</returns>
    Task<Result<List<PlexServerStatus>>> CheckAllConnectionsOfPlexServerAsync(int plexServerId);

    Task<Result<PlexServerConnection>> GetPlexServerConnectionAsync(int plexServerConnectionId);

    Task<Result<List<PlexServerConnection>>> GetAllPlexServerConnectionsAsync();
    Task<Result> CheckAllConnectionsOfPlexServersByAccountIdAsync(int plexAccountId);

    #endregion

    #endregion
}