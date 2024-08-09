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
/// <returns></returns>
public record InspectAllPlexServersByAccountIdCommand(int PlexAccountId) : IRequest<Result>;

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

        var refreshResult = await _mediator.Send(
            new QueueRefreshPlexServerAccessJobCommand(plexAccountId),
            cancellationToken
        );
        if (refreshResult.IsFailed)
            return refreshResult.LogError();

        // Await job running
        await _scheduler.AwaitJobRunning(refreshResult.Value, cancellationToken);

        // Retrieve all accessible servers for the PlexAccount
        var plexAccountDisplayName = await _dbContext.GetPlexAccountDisplayName(plexAccountId, cancellationToken);

        var plexServers = await _dbContext.GetAccessiblePlexServers(plexAccountId, cancellationToken);
        if (plexServers.IsFailed)
            return plexServers.LogError();

        if (!plexServers.Value.Any())
            return Result.Ok();

        // Inspect all PlexServers
        await Task.WhenAll(
            plexServers.Value.Select(x =>
                _mediator.Send(new QueueInspectPlexServerJobCommand(x.Id), CancellationToken.None)
            )
        );

        _log.Here()
            .Information(
                "Successfully finished the inspection of all plexServers related to {NameOfPlexAccount} {PlexAccountDisplayName}",
                nameof(PlexAccount),
                plexAccountDisplayName
            );

        return Result.Ok();
    }
}
