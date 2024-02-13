using Application.Contracts;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;

namespace PlexRipper.Application;

/// <summary>
/// Inspects the <see cref="PlexServer">PlexServers</see> for connectivity.
/// When successfully connected, the <see cref="PlexLibrary">PlexLibraries</see> are stored in the database.
/// </summary>
/// <param name="PlexAccountId">The id of the <see cref="PlexAccount"/> to check for.</param>
/// <param name="SkipRefreshAccessibleServers"></param>
/// <returns></returns>
public record InspectAllPlexServersByAccountIdCommand(int PlexAccountId, bool SkipRefreshAccessibleServers = false) : IRequest<Result>;

public class InspectAllPlexServersByAccountIdCommandValidator : AbstractValidator<InspectAllPlexServersByAccountIdCommand>
{
    public InspectAllPlexServersByAccountIdCommandValidator()
    {
        RuleFor(x => x.PlexAccountId).GreaterThan(0);
    }
}

public class InspectAllPlexServersByAccountIdCommandHandler : IRequestHandler<InspectAllPlexServersByAccountIdCommand, Result>
{
    private readonly ILog _log;
    private readonly IMediator _mediator;
    private readonly IPlexServerConnectionsService _plexServerConnectionsService;
    private readonly IPlexRipperDbContext _dbContext;

    public InspectAllPlexServersByAccountIdCommandHandler(
        ILog log,
        IMediator mediator,
        IPlexServerConnectionsService plexServerConnectionsService,
        IPlexRipperDbContext dbContext)
    {
        _log = log;
        _mediator = mediator;
        _plexServerConnectionsService = plexServerConnectionsService;
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(InspectAllPlexServersByAccountIdCommand command, CancellationToken cancellationToken)
    {
        var plexAccount = await _dbContext.PlexAccounts.FindAsync(command.PlexAccountId, cancellationToken);
        var plexServers = await _dbContext.GetAccessiblePlexServers(command.PlexAccountId, cancellationToken);

        if (!plexServers.Value.Any())
        {
            _log.Warning("No accessible PlexServers found for PlexAccount: {PlexAccountDisplayName}", plexAccount.DisplayName);
            return Result.Ok();
        }

        _log.Here()
            .Information("Inspecting {PlexServersCount} PlexServers for PlexAccount: {PlexAccountDisplayName}", plexServers.Value.Count,
                plexAccount.DisplayName);

        if (!command.SkipRefreshAccessibleServers)
        {
            var refreshResult = await _mediator.Send(new RefreshAccessiblePlexServersCommand(plexAccount.Id), cancellationToken);
            if (refreshResult.IsFailed)
                return refreshResult.ToResult();
        }
        else
        {
            _log.Here()
                .Warning("Skipping refreshing of the accessible plex server in {NameOfInspectAllPlexServersByAccountId}",
                    nameof(InspectAllPlexServersByAccountIdCommand));
        }

        // Create connection check tasks for all connections
        var checkResult = await _plexServerConnectionsService.CheckAllConnectionsOfPlexServersByAccountIdAsync(plexAccount.Id);
        if (checkResult.IsFailed)
            return checkResult;

        await RetrieveAllAccessibleLibrariesAsync(plexAccount.Id);

        // Sync libraries
        foreach (var plexServer in plexServers.Value)
            await _mediator.Send(new QueueSyncServerJobCommand(plexServer.Id, true), cancellationToken);


        _log.Here()
            .Information("Successfully finished the inspection of all plexServers related to {NameOfPlexAccount} {PlexAccountDisplayName}", nameof(PlexAccount),
                plexAccount.DisplayName);

        return Result.Ok();
    }

    private async Task RetrieveAllAccessibleLibrariesAsync(int plexAccountId)
    {
        _log.Information("Retrieving accessible Plex libraries for Plex account with id {PlexAccountId}", plexAccountId);

        var plexServersResult = await _mediator.Send(new GetAllPlexServersByPlexAccountIdQuery(plexAccountId));
        if (plexServersResult.IsFailed)
        {
            plexServersResult.LogError();
            return;
        }

        var retrieveTasks = plexServersResult.Value
            .Select(async plexServer => await _mediator.Send(new RefreshLibraryAccessCommand(plexAccountId, plexServer.Id)));

        await Task.WhenAll(retrieveTasks);
    }
}