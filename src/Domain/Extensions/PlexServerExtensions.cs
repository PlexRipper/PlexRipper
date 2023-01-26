using Logging.Interface;

namespace PlexRipper.Domain;

public static class PlexServerExtensions
{
    private static readonly ILog _log = LogConfig.GetLog(typeof(PlexServerExtensions));

    /// <summary>
    /// Gets the server url based on the available connections, e.g: http://112.202.10.213:32400.
    /// </summary>
    /// <param name="plexServerConnectionId">The optional <see cref="PlexServerConnections"/> to use.</param>
    /// <returns>The connection url based on preference or on fallback.</returns>
    public static string GetServerUrl(this PlexServer plexServer, int plexServerConnectionId = 0)
    {
        if (!plexServer.PlexServerConnections.Any())
            throw new Exception($"PlexServer with id {plexServer.Id} and name {plexServer.Name} has no connections available!");

        if (plexServerConnectionId > 0)
        {
            var connection = plexServer.PlexServerConnections.Find(x => x.Id == plexServerConnectionId);
            if (connection is not null)
                return connection.Url;

            _log.Warning("Could not find parameter {nameof(plexServerConnectionId)} with id {PlexServerConnectionId} for server {Name}",
                nameof(plexServerConnectionId), plexServerConnectionId, plexServer.Name, 0);
        }

        if (plexServer.PreferredConnectionId > 0)
        {
            var connection = plexServer.PlexServerConnections.Find(x => x.Id == plexServer.PreferredConnectionId);
            if (connection is not null)
                return connection.Url;

            _log.Warning("Could not find preferred connection with id {PreferredConnectionId} for server {Name}", plexServer.PreferredConnectionId,
                plexServer.Name, 0);
        }
        else
        {
            var connection = plexServer.PlexServerConnections.Find(x => x.Address == plexServer.PublicAddress);
            if (connection is not null)
                return connection.Url;
        }

        _log.Warning("Could not find connection based on public address: {PublicAddress} for server {Name}", plexServer.PublicAddress, plexServer.Name, 0);
        _log.Warning("Trying the first connection: {PUrl}", plexServer.PlexServerConnections.First().Url);
        return plexServer.PlexServerConnections.First().Url;
    }
}