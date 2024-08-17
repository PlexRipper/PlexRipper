using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record AddOrUpdatePlexAccountServersCommand(int PlexAccountId, List<ServerAccessTokenDTO> ServerAccessTokens)
    : IRequest<Result>;

public class AddOrUpdatePlexAccountServersCommandValidator : AbstractValidator<AddOrUpdatePlexAccountServersCommand>
{
    public AddOrUpdatePlexAccountServersCommandValidator()
    {
        RuleFor(x => x.PlexAccountId).GreaterThan(0);
    }
}

public class AddOrUpdatePlexAccountServersCommandHandler : IRequestHandler<AddOrUpdatePlexAccountServersCommand, Result>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    public AddOrUpdatePlexAccountServersCommandHandler(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(AddOrUpdatePlexAccountServersCommand command, CancellationToken cancellationToken)
    {
        var plexAccountId = command.PlexAccountId;
        var serverAccessTokens = command.ServerAccessTokens;

        // Ensure we don't have any empty access tokens
        foreach (var serverAccessToken in serverAccessTokens)
            if (string.IsNullOrWhiteSpace(serverAccessToken.AccessToken))
            {
                _log.Error(
                    "Server Access Token was given with an empty access token for machine identifier: {MachineIdentifier}",
                    serverAccessToken.MachineIdentifier
                );
            }

        serverAccessTokens.RemoveAll(x => string.IsNullOrWhiteSpace(x.AccessToken));

        var plexAccount = await _dbContext.PlexAccounts.GetAsync(plexAccountId, cancellationToken);
        if (plexAccount is null)
            return ResultExtensions.EntityNotFound(nameof(PlexAccount), plexAccountId);

        // Add or update the PlexAccount and PlexServer relationships
        _log.InformationLine("Adding or updating the PlexAccount association with PlexServers now");

        // Fetch all relevant PlexServers in one query
        var machineIdentifiers = serverAccessTokens.Select(x => x.MachineIdentifier).ToList();
        var plexServers = await _dbContext
            .PlexServers.Where(x => machineIdentifiers.Contains(x.MachineIdentifier))
            .ToDictionaryAsync(x => x.MachineIdentifier, cancellationToken);

        var newAccountServers = new List<PlexAccountServer>();
        var accessiblePlexServers = new List<int>();

        foreach (var serverAccessToken in serverAccessTokens)
        {
            if (!plexServers.TryGetValue(serverAccessToken.MachineIdentifier, out var plexServer))
            {
                _log.ErrorLine(
                    "Server Access Token was given for a machine identifier that has no PlexServer in the database: {MachineIdentifier}",
                    serverAccessToken.MachineIdentifier
                );
                continue;
            }

            accessiblePlexServers.Add(plexServer.Id);

            // Check if this PlexAccount has been associated with the plexServer already
            var plexAccountServer = await _dbContext.PlexAccountServers.FirstOrDefaultAsync(
                x => x.PlexAccountId == plexAccountId && x.PlexServerId == plexServer.Id,
                cancellationToken
            );

            if (plexAccountServer is null)
            {
                _log.Here()
                    .Debug(
                        "PlexAccount {PlexAccountDisplayName} does not have an association with PlexServer: {PlexServerName}, creating one now with the authentication token",
                        plexAccount.DisplayName,
                        plexServer.Name
                    );
                newAccountServers.Add(
                    new PlexAccountServer
                    {
                        PlexAccountId = plexAccountId,
                        PlexServerId = plexServer.Id,
                        AuthToken = serverAccessToken.AccessToken,
                        AuthTokenCreationDate = DateTime.UtcNow,
                    }
                );
            }
            else
            {
                _log.Here()
                    .Debug(
                        "PlexAccount {PlexAccountDisplayName} already has an association with PlexServer: {PlexServerName}, updating authentication token now",
                        plexAccount.DisplayName,
                        plexServer.Name
                    );
                plexAccountServer.AuthToken = serverAccessToken.AccessToken;
                plexAccountServer.AuthTokenCreationDate = DateTime.UtcNow;
            }
        }

        // Add new associations in bulk
        if (newAccountServers.Any())
            await _dbContext.PlexAccountServers.AddRangeAsync(newAccountServers, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _log.InformationLine("Checking if there are any PlexServers this PlexAccount has no access to anymore");

        // The list of all past and current serverId's the plexAccount has access too
        var removalList = await _dbContext
            .PlexAccountServers.Where(x =>
                x.PlexAccountId == plexAccountId && !accessiblePlexServers.Contains(x.PlexServerId)
            )
            .Include(x => x.PlexAccount)
            .Include(x => x.PlexServer)
            .AsTracking()
            .ToListAsync(cancellationToken);

        if (removalList.Any())
        {
            foreach (var plexAccountServer in removalList)
                _log.Here()
                    .Warning(
                        "PlexAccount {PlexAccountDisplayName} has lost access to {PlexServerName}!",
                        plexAccountServer.PlexAccount!.DisplayName,
                        plexAccountServer.PlexServer!.Name
                    );
            var removalIds = removalList.Select(x => x.PlexServerId).ToList();
            await _dbContext
                .PlexAccountServers.Where(x => removalIds.Contains(x.PlexServerId) && x.PlexAccountId == plexAccountId)
                .ExecuteDeleteAsync(cancellationToken);
        }
        else
        {
            _log.Information(
                "No Plex server access for {PlexAccountDisplayName} has been lost",
                plexAccount.DisplayName
            );
        }

        return Result.Ok();
    }
}
