using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexApi.Contracts;

namespace PlexRipper.Application;

/// <summary>
/// Retrieve the latest accessible <see cref="PlexServer">PlexServers</see> for this <see cref="PlexAccount"/> from the PlexAPI and stores it in the Database.
/// </summary>
/// <param name="PlexAccountId">The id of the <see cref="PlexAccount"/> to check.</param>
public record RefreshPlexServerAccessCommand(int PlexAccountId) : ICommand<Result<RefreshPlexServerAccessRapport>>;

public class RefreshPlexServerAccessCommandValidator : AbstractValidator<RefreshPlexServerAccessCommand>
{
    public RefreshPlexServerAccessCommandValidator()
    {
        RuleFor(x => x.PlexAccountId).GreaterThan(0);
    }
}

public class RefreshPlexServerAccessCommandHandler
    : ICommandHandler<RefreshPlexServerAccessCommand, Result<RefreshPlexServerAccessRapport>>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    public RefreshPlexServerAccessCommandHandler(
        ILog log,
        IPlexRipperDbContext dbContext,
        ICommandExecutor commandExecutor
    )
    {
        _log = log;
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    public async Task<Result<RefreshPlexServerAccessRapport>> ExecuteAsync(
        RefreshPlexServerAccessCommand command,
        CancellationToken cancellationToken
    )
    {
        var plexAccountId = command.PlexAccountId;

        var plexAccountName = await _dbContext.GetPlexAccountDisplayName(plexAccountId, cancellationToken);

        _log.Debug("Refreshing Plex servers access for PlexAccount: {PlexAccountName}", plexAccountName);

        var result = await _commandExecutor.Send(new GetAccessiblePlexServersCommand(plexAccountId), cancellationToken);

        // If the Plex API returns a 401 Unauthorized error, remove the PlexAccount and PlexServerAccess
        if (result.HasPlex401UnauthorizedError())
        {
            _log.Warning(
                "Plex API returned 401 Unauthorized for PlexAccount: {PlexAccountDisplayName}",
                plexAccountName
            );
            _log.Warning(
                "Removing PlexServerAccess and LibraryAccess for PlexAccount: {PlexAccountDisplayName}",
                plexAccountName
            );

            _log.Error("{PlexAccountName} token has been invalidated and has lost Plex Server access", plexAccountName);
            return await RemovePlexAccess(plexAccountId);
        }

        if (result.IsFailed)
            return result.LogError();

        if (!result.Value.Any())
        {
            _log.Warning("No Plex servers found for PlexAccount: {plexAccountName}", plexAccountName);
            return await RemovePlexAccess(plexAccountId);
        }

        var serverList = result.Value.Select(x => x.PlexServer).ToList();
        var serverAccessTokens = result.Value.Select(x => x.AccessToken).ToList();

        // Add PlexServers and their PlexServerConnections
        var updateResult = await _commandExecutor.Send(
            new AddOrUpdatePlexServersCommand(serverList),
            CancellationToken.None
        );
        if (updateResult.IsFailed)
            return updateResult.LogError();

        // Add or update the PlexAccount and PlexServer relationships
        var plexServerAccountAccessRapport = await _commandExecutor.Send(
            new AddOrUpdatePlexAccountServersCommand(plexAccountId, serverAccessTokens),
            cancellationToken
        );

        if (plexServerAccountAccessRapport.IsFailed)
            return plexServerAccountAccessRapport.LogError();

        _log.Information(
            "Successfully refreshed accessible Plex servers for account {PlexAccountDisplayName}",
            plexAccountName
        );

        return plexServerAccountAccessRapport;
    }

    /// <summary>
    ///  Remove all PlexServerAccess and LibraryAccess for the given PlexAccount
    /// </summary>
    /// <param name="plexAccountId"></param>
    private async Task<Result<RefreshPlexServerAccessRapport>> RemovePlexAccess(int plexAccountId)
    {
        var plexServers = await _dbContext
            .PlexAccountServers.Include(x => x.PlexServer)
            .Where(x => x.PlexAccountId == plexAccountId)
            .Select(x => new { x.PlexServerId, x.PlexServer!.Name })
            .ToListAsync();

        var plexAccountName = await _dbContext.GetPlexAccountDisplayName(plexAccountId, CancellationToken.None);
        var rapport = new RefreshPlexServerAccessRapport(plexAccountId, plexAccountName);

        if (!plexServers.Any())
            return Result.Ok(rapport);

        foreach (var plexServer in plexServers)
        {
            rapport.Access.Add(
                new RefreshPlexServerAccessRapportRow(PlexAccessState.Revoked, plexServer.PlexServerId, plexServer.Name)
            );
        }

        await _dbContext
            .PlexAccountServers.Where(x => x.PlexAccountId == plexAccountId)
            .ExecuteDeleteAsync(CancellationToken.None);

        return Result.Ok(rapport);
    }
}
