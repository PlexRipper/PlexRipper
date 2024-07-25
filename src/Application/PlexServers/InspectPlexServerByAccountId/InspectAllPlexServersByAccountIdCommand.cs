using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Quartz;

namespace PlexRipper.Application;

/// <summary>
/// Inspects the <see cref="PlexServer">PlexServers</see> for connectivity.
/// When successfully connected, the <see cref="PlexLibrary">PlexLibraries</see> are stored in the database.
/// </summary>
/// <param name="PlexAccountId">The id of the <see cref="PlexAccount"/> to check for.</param>
/// <param name="SkipRefreshAccessibleServers"></param>
/// <returns></returns>
public record InspectAllPlexServersByAccountIdCommand(int PlexAccountId, bool SkipRefreshAccessibleServers = false)
    : IRequest<Result>;

public class InspectAllPlexServersByAccountIdCommandValidator
    : AbstractValidator<InspectAllPlexServersByAccountIdCommand>
{
    public InspectAllPlexServersByAccountIdCommandValidator()
    {
        RuleFor(x => x.PlexAccountId).GreaterThan(0);
    }
}

public class InspectAllPlexServersByAccountIdCommandHandler
    : IRequestHandler<InspectAllPlexServersByAccountIdCommand, Result>
{
    private readonly ILog _log;
    private readonly IMediator _mediator;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IScheduler _scheduler;

    public InspectAllPlexServersByAccountIdCommandHandler(
        ILog log,
        IMediator mediator,
        IPlexRipperDbContext dbContext,
        IScheduler scheduler
    )
    {
        _log = log;
        _mediator = mediator;
        _dbContext = dbContext;
        _scheduler = scheduler;
    }

    public async Task<Result> Handle(
        InspectAllPlexServersByAccountIdCommand command,
        CancellationToken cancellationToken
    )
    {
        var plexAccountId = command.PlexAccountId;

        if (!command.SkipRefreshAccessibleServers)
        {
            var refreshResult = await _mediator.Send(
                new QueueRefreshPlexServerAccessJobCommand(plexAccountId),
                cancellationToken
            );
            if (refreshResult.IsFailed)
                return refreshResult.LogError();

            // Await job running
            await _scheduler.AwaitJobRunning(refreshResult.Value, cancellationToken);
        }
        else
        {
            _log.Here()
                .Warning(
                    "Skipping refreshing of the accessible plex server in {NameOfInspectAllPlexServersByAccountId}",
                    nameof(InspectAllPlexServersByAccountIdCommand)
                );
        }

        // Retrieve all accessible servers for the PlexAccount
        var plexAccount = await _dbContext.PlexAccounts.GetAsync(plexAccountId, cancellationToken);
        if (plexAccount is null)
            return ResultExtensions.EntityNotFound(nameof(PlexAccount), plexAccountId).LogError();

        var plexServers = await _dbContext.GetAccessiblePlexServers(plexAccountId, cancellationToken);
        if (plexServers.IsFailed)
            return plexServers.LogError();

        _log.Here()
            .Information(
                "Inspecting {PlexServersCount} PlexServers for PlexAccount: {PlexAccountDisplayName}",
                plexServers.Value.Count,
                plexAccount.DisplayName
            );

        if (!plexServers.Value.Any())
        {
            _log.Warning(
                "No accessible PlexServers found for PlexAccount: {PlexAccountDisplayName}",
                plexAccount.DisplayName
            );
            return Result.Ok();
        }

        // Create connection check tasks for all connections

        var plexServerIds = plexServers.Value.Select(x => x.Id).ToList();
        var checkResult = await _mediator.Send(
            new QueueCheckPlexServerConnectionsJobCommand(plexServerIds),
            cancellationToken
        );

        await _scheduler.AwaitJobRunning(checkResult.Value, CancellationToken.None);

        // Retrieve all accessible libraries
        await RetrieveAllAccessibleLibrariesAsync(plexAccount.Id);

        // Sync libraries
        foreach (var plexServer in plexServers.Value)
            await _mediator.Send(new QueueSyncServerJobCommand(plexServer.Id, true), CancellationToken.None);

        _log.Here()
            .Information(
                "Successfully finished the inspection of all plexServers related to {NameOfPlexAccount} {PlexAccountDisplayName}",
                nameof(PlexAccount),
                plexAccount.DisplayName
            );

        return Result.Ok();
    }

    private async Task RetrieveAllAccessibleLibrariesAsync(int plexAccountId)
    {
        _log.Information(
            "Retrieving accessible Plex libraries for Plex account with id {PlexAccountId}",
            plexAccountId
        );

        var plexServers = await _dbContext.GetAllPlexServersByPlexAccountIdQuery(plexAccountId);

        var retrieveTasks = plexServers.Select(async plexServer =>
            await _mediator.Send(new RefreshLibraryAccessCommand(plexAccountId, plexServer.Id))
        );

        await Task.WhenAll(retrieveTasks);
    }
}
