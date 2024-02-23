using Application.Contracts;
using Data.Contracts;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public class AddOrUpdatePlexAccountServersCommand : IAddOrUpdatePlexAccountServersCommand
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    public AddOrUpdatePlexAccountServersCommand(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public async Task<Result> ExecuteAsync(
        int plexAccountId,
        List<ServerAccessTokenDTO> serverAccessTokens,
        CancellationToken cancellationToken = default)
    {
        var plexAccount = await _dbContext.PlexAccounts.GetAsync(plexAccountId, cancellationToken);
        if (plexAccount is null)
            return ResultExtensions.EntityNotFound(nameof(PlexAccount), plexAccountId);

        // Add or update the PlexAccount and PlexServer relationships
        _log.InformationLine("Adding or updating the PlexAccount association with PlexServers now");
        var accessiblePlexServers = new List<int>();
        foreach (var serverAccessToken in serverAccessTokens)
        {
            var plexServer = _dbContext.PlexServers.FirstOrDefault(x => x.MachineIdentifier == serverAccessToken.MachineIdentifier);
            if (plexServer is null)
            {
                _log.ErrorLine("Server Access Token was given for a machine identifier that has no PlexServer in the database");
                continue;
            }

            accessiblePlexServers.Add(plexServer.Id);

            // Check if this PlexAccount has been associated with the plexServer already
            var plexAccountServer = await _dbContext.PlexAccountServers
                .Where(x => x.PlexAccountId == plexAccountId && x.PlexServerId == plexServer.Id)
                .AsTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (plexAccountServer is null)
            {
                // Create entry
                _log.Here()
                    .Debug(
                        "PlexAccount {PlexAccountDisplayName} does not have an association with PlexServer: {PlexServerName}, creating one now with the authentication token",
                        plexAccount.DisplayName, plexServer.Name);
                var accountServerEntry = new PlexAccountServer
                {
                    PlexAccountId = plexAccountId,
                    PlexServerId = plexServer.Id,
                    AuthToken = serverAccessToken.AccessToken,
                    AuthTokenCreationDate = DateTime.UtcNow,
                };
                await _dbContext.PlexAccountServers.AddAsync(accountServerEntry, cancellationToken);
            }
            else
            {
                // Update entry
                _log.Here()
                    .Debug(
                        "PlexAccount {PlexAccountDisplayName} already has an association with PlexServer: {PlexServerName}, updating authentication token now",
                        plexAccount.DisplayName, plexServer.Name);
                plexAccountServer.AuthToken = serverAccessToken.AccessToken;
                plexAccountServer.AuthTokenCreationDate = DateTime.UtcNow;
            }
        }

        _log.InformationLine("Checking if there are any PlexServers this PlexAccount has no access to anymore");

        // The list of all past and current serverId's the plexAccount has access too
        var removalList = await _dbContext.PlexAccountServers
            .Where(x => x.PlexAccountId == plexAccountId && !accessiblePlexServers.Contains(x.PlexServerId))
            .Include(x => x.PlexServer)
            .Include(x => x.PlexAccount)
            .ToListAsync(cancellationToken);

        if (removalList.Any())
        {
            foreach (var plexAccountServer in removalList)
            {
                _log.Here()
                    .Warning("PlexAccount {PlexAccountDisplayName} has lost access to {PlexServerName}!", plexAccountServer.PlexAccount.DisplayName,
                        plexAccountServer.PlexServer.Name);
                _dbContext.Entry(plexAccountServer).State = EntityState.Deleted;
            }
        }
        else
            _log.Information("No Plex server access for {PlexAccountDisplayName} has been lost", plexAccount.DisplayName);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}