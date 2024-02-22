using FluentResults;
using Logging;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

public static class PlexServerConnectionExtensions
{
    private static readonly ILog _log = LogManager.CreateLogInstance(typeof(PlexServerConnectionExtensions));

    public static async Task<Result<PlexServerConnection>> GetValidPlexServerConnection(
        this IPlexRipperDbContext dbContext,
        int plexServerId,
        CancellationToken cancellationToken = default)
    {
        var plexServer = await dbContext
            .PlexServers
            .Include(x => x.PlexServerConnections)
            .ThenInclude(x => x.PlexServerStatus.OrderByDescending(y => y.LastChecked).Take(5))
            .FirstOrDefaultAsync(x => x.Id == plexServerId, cancellationToken);

        var plexServerConnections = plexServer.PlexServerConnections;
        if (!plexServerConnections.Any())
            return Result.Fail($"PlexServer with id {plexServer.Id} and name {plexServer.Name} has no connections available!").LogError();

        // Find based on preference
        if (plexServer.PreferredConnectionId > 0)
        {
            var connection = plexServerConnections.Find(x => x.Id == plexServer.PreferredConnectionId);
            if (connection is not null)
                return Result.Ok(connection);

            _log.Here()
                .Verbose("Could not find preferred connection with id {PlexServerConnectionId} for server {PlexServerName}", plexServer.PreferredConnectionId,
                    plexServer.Name);
        }

        // Find based on public address
        _log.Here().Verbose("Attempting to find PlexServerConnection that matches the PlexServer public address: {PublicAddress}", plexServer.PublicAddress);

        var publicConnection = plexServerConnections.Find(x => x.Address == plexServer.PublicAddress);
        if (publicConnection is not null)
            return Result.Ok(publicConnection);

        _log.Here()
            .Verbose("Could not find connection based on public address: {PublicAddress} for server {PlexServerName}", plexServer.PublicAddress,
                plexServer.Name);

        // Find based on what's successful
        var successPlexServerConnections = plexServerConnections
            .Where(x => x.LatestConnectionStatus?.IsSuccessful ?? false)
            .ToList();
        if (successPlexServerConnections.Any())
            return Result.Ok(successPlexServerConnections.First());

        // Find anything...
        _log.Verbose("Could not find a recent successful connection. We're just gonna YOLO this and pick the first connection: {PlexServerConnectionUrl}",
            plexServerConnections.First());
        return Result.Ok(plexServerConnections.First());
    }
}