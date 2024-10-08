using FluentResults;
using Logging;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

public static partial class DbContextExtensions
{
    private static readonly ILog _log = LogManager.CreateLogInstance(typeof(DbContextExtensions));

    public static async Task<Result<PlexServerConnection>> ChoosePlexServerConnection(
        this IPlexRipperDbContext dbContext,
        int plexServerId,
        CancellationToken cancellationToken = default
    )
    {
        if (plexServerId <= 0)
            return ResultExtensions.IsInvalidId(nameof(PlexServer), plexServerId);

        var plexServer = await dbContext
            .PlexServers.Include(x => x.PlexServerConnections)
            .ThenInclude(x => x.PlexServerStatus.OrderByDescending(y => y.LastChecked).Take(5))
            .FirstOrDefaultAsync(x => x.Id == plexServerId, cancellationToken);

        if (plexServer is null)
            return ResultExtensions.EntityNotFound(nameof(PlexServer), plexServerId);

        var plexServerConnections = plexServer.PlexServerConnections;
        if (!plexServerConnections.Any())
        {
            return Result
                .Fail($"PlexServer with id {plexServer.Id} and name {plexServer.Name} has no connections available!")
                .LogError();
        }

        if (plexServerConnections.All(x => !x.IsOnline))
        {
            var msg = _log.Here()
                .Error(
                    "PlexServer with id {plexServerId} and name {PlexServerName} has no online connections available!",
                    plexServer.Id,
                    plexServer.Name
                )
                .ToLogString();
            return Result.Fail(msg);
        }

        var successPlexServerConnections = plexServerConnections.Where(x => x.IsOnline).ToList();

        // Find based on preference
        if (plexServer.PreferredConnectionId > 0)
        {
            var connection = successPlexServerConnections.Find(x => x.Id == plexServer.PreferredConnectionId);
            if (connection is not null)
                return Result.Ok(connection);

            _log.Here()
                .Verbose(
                    "Could not find preferred connection with id {PlexServerConnectionId} for server {PlexServerName}",
                    plexServer.PreferredConnectionId,
                    plexServer.Name
                );
        }

        // Find based on online local address, because that is the fastest
        _log.Here()
            .Verbose(
                "Attempting to find a local PlexServerConnection that is online for server {PlexServerName}",
                plexServer.Name
            );

        var localConnection = successPlexServerConnections.FirstOrDefault(x => x.Local);
        if (localConnection is not null)
            return Result.Ok(localConnection);

        // Find based on public address
        _log.Here()
            .Verbose(
                "Attempting to find PlexServerConnection that matches the PlexServer public address: {PublicAddress}",
                plexServer.PublicAddress
            );
        var publicConnection = successPlexServerConnections.FirstOrDefault(x => x.Address == plexServer.PublicAddress);
        if (publicConnection is not null)
            return Result.Ok(publicConnection);

        // Give preference to non-PlexTv connections
        var directConnections = successPlexServerConnections.Where(x => !x.IsPlexTvConnection).ToList();
        if (directConnections.Any())
        {
            _log.Verbose(
                "Found a direct connection that was successful. We're gonna use that: {DirectConnection}",
                directConnections.First()
            );
            return Result.Ok(directConnections.First());
        }

        return Result.Ok(successPlexServerConnections.First());
    }

    /// <summary>
    /// Returns an authentication token needed to authenticate communication with the <see cref="PlexServer" />.
    /// Note: An PlexAccountId of 0 can be passed to automatically retrieve first a non-main account token, and if not found a main account server token.
    /// </summary>
    /// <param name="dbContext"> The <see cref="IPlexRipperDbContext" /> to retrieve the token from.</param>
    /// <param name="plexServerId">The id of the <see cref="PlexServer" /> to retrieve a token for.</param>
    /// <param name="cancellationToken"> The cancellation token to cancel operation.</param>
    public static Task<Result<string>> GetPlexServerTokenAsync(
        this IPlexRipperDbContext dbContext,
        int plexServerId,
        CancellationToken cancellationToken = default
    ) => GetPlexServerTokenAsync(dbContext, plexServerId, 0, cancellationToken);

    /// <summary>
    ///  Returns the authentication token needed to authenticate communication with the <see cref="PlexServer" />.
    /// Note: An PlexAccountId of 0 can be passed to automatically retrieve first a non-main account token, and if not found a main account server token.
    /// </summary>
    /// <param name="dbContext">The <see cref="IPlexRipperDbContext" /> to retrieve the token from.</param>
    /// <param name="plexServerId">The id of the <see cref="PlexServer" /> to retrieve a token for.</param>
    /// <param name="plexAccountId"> An PlexAccountId of 0 can be passed to automatically retrieve first a non-main account token, and if not found a main account server token.</param>
    /// <param name="cancellationToken"> The cancellation token to cancel operation.</param>
    public static async Task<Result<string>> GetPlexServerTokenAsync(
        this IPlexRipperDbContext dbContext,
        int plexServerId,
        int plexAccountId = 0,
        CancellationToken cancellationToken = default
    )
    {
        // Attempt to find a non-main account token first
        if (plexAccountId == 0)
        {
            var nonMainServerToken = await dbContext
                .PlexAccountServers.Include(x => x.PlexAccount)
                .FirstOrDefaultAsync(x => x.PlexServerId == plexServerId && !x.PlexAccount!.IsMain, cancellationToken);

            // Check if we have access with a non-main account
            if (nonMainServerToken != null)
                return Result.Ok(nonMainServerToken.AuthToken);

            // Fallback to a main-account access
            var mainServerToken = await dbContext
                .PlexAccountServers.Include(x => x.PlexAccount)
                .FirstOrDefaultAsync(x => x.PlexServerId == plexServerId, cancellationToken);

            if (mainServerToken != null)
                return Result.Ok(mainServerToken.AuthToken);

            return Result
                .Fail($"Could not find any authenticationToken for PlexServer with id: {plexServerId}")
                .LogError();
        }

        var authToken = await dbContext.PlexAccountServers.FirstOrDefaultAsync(
            x => x.PlexAccountId == plexAccountId && x.PlexServerId == plexServerId,
            cancellationToken
        );

        if (authToken != null)
            return Result.Ok(authToken.AuthToken);

        return Result
            .Fail(
                new Error(
                    $"Could not find an authenticationToken for PlexAccount with id: {plexAccountId} and PlexServer with id: {plexServerId}"
                )
            )
            .LogError();
    }
}
