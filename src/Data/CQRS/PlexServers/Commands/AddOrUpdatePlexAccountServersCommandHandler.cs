using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexServers;

public class AddOrUpdatePlexAccountServersValidator : AbstractValidator<AddOrUpdatePlexAccountServersCommand>
{
    public AddOrUpdatePlexAccountServersValidator()
    {
        RuleFor(x => x.PlexAccount).NotNull();
        RuleFor(x => x.PlexAccount.Id).GreaterThan(0);
        RuleFor(x => x.ServerAccessTokens).NotNull();
    }
}

public class AddOrUpdatePlexAccountServersCommandHandler : BaseHandler, IRequestHandler<AddOrUpdatePlexAccountServersCommand, Result>
{
    public AddOrUpdatePlexAccountServersCommandHandler(ILog log, PlexRipperDbContext dbContext) : base(log, dbContext) { }

    public async Task<Result> Handle(AddOrUpdatePlexAccountServersCommand command, CancellationToken cancellationToken)
    {
        var plexAccount = command.PlexAccount;
        var serverAccessTokens = command.ServerAccessTokens;

        // Add or update the PlexAccount and PlexServer relationships
        _log.Information("Adding or updating the PlexAccount association with PlexServers now", 0);
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
                .Where(x => x.PlexAccountId == plexAccount.Id && x.PlexServerId == plexServer.Id)
                .AsTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (plexAccountServer is null)
            {
                // Create entry
                _log.Debug(
                    "PlexAccount {PlexAccountDisplayName} does not have an association with PlexServer: {PlexServerName}, creating one now with the authentication token",
                    plexAccount.DisplayName, plexServer.Name, 0);
                var accountServerEntry = new PlexAccountServer
                {
                    PlexAccountId = plexAccount.Id,
                    PlexServerId = plexServer.Id,
                    AuthToken = serverAccessToken.AccessToken,
                    AuthTokenCreationDate = DateTime.UtcNow,
                };
                await _dbContext.PlexAccountServers.AddAsync(accountServerEntry, cancellationToken);
            }
            else
            {
                // Update entry
                _log.Debug(
                    "PlexAccount {PlexAccountDisplayName}  already has an association with PlexServer: {PlexServerName}, updating authentication token now",
                    plexAccount.DisplayName, plexServer.Name, 0);
                plexAccountServer.AuthToken = serverAccessToken.AccessToken;
                plexAccountServer.AuthTokenCreationDate = DateTime.UtcNow;
            }
        }

        _log.Information("Checking if there are any PlexServers this PlexAccount has no access to anymore", 0);

        // The list of all past and current serverId's the plexAccount has access too
        var removalList = await _dbContext.PlexAccountServers
            .Where(x => x.PlexAccountId == plexAccount.Id && !accessiblePlexServers.Contains(x.PlexServerId))
            .Include(x => x.PlexServer)
            .Include(x => x.PlexAccount)
            .ToListAsync(cancellationToken);

        if (removalList.Any())
        {
            foreach (var plexAccountServer in removalList)
            {
                _log.Warning("PlexAccount {DisplayName} has lost access to {PlexServerName}!", plexAccountServer.PlexAccount.DisplayName,
                    plexAccountServer.PlexServer.Name, 0);
                _dbContext.Entry(plexAccountServer).State = EntityState.Deleted;
            }
        }
        else
            _log.Information("No Plex server access for {PlexAccountDisplayName} has been lost", plexAccount.DisplayName);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}