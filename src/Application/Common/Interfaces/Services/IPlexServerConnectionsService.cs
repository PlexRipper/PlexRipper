namespace PlexRipper.Application;

public interface IPlexServerConnectionsService
{
    Task<Result<PlexServerStatus>> CheckPlexServerConnection(PlexServerConnection plexServerConnectionId);
    Task<Result<PlexServerStatus>> CheckPlexServerConnection(int plexServerConnectionId);

    /// <summary>
    /// Checks if the <see cref="PlexServerConnection"/> is available and log the status in the database.
    /// </summary>
    /// <param name="plexServerConnectionId">The <see cref="PlexServerConnection"/> to check for connectivity.</param>
    /// <param name="trimEntries">Delete entries which are older than a certain threshold.</param>
    /// <returns>The latest <see cref="PlexServerStatus"/>.</returns>
    Task<Result<PlexServerStatus>> CheckPlexServerConnectionStatusAsync(
        int plexServerConnectionId,
        bool trimEntries = true);

    Task<Result<PlexServerConnection>> GetPlexServerConnectionAsync(int plexServerConnectionId);

    Task<Result<List<PlexServerConnection>>> GetAllPlexServerConnectionsAsync();
}