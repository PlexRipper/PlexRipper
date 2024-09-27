using Data.Contracts;
using FluentValidation;
using Logging.Interface;

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

    public InspectAllPlexServersByAccountIdCommandHandler(ILog log, IMediator mediator, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _mediator = mediator;
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(
        InspectAllPlexServersByAccountIdCommand command,
        CancellationToken cancellationToken
    )
    {
        var plexAccountId = command.PlexAccountId;

        var refreshResult = await _mediator.Send(new RefreshPlexServerAccessCommand(plexAccountId), cancellationToken);
        if (refreshResult.IsFailed)
            return refreshResult.LogError();

        var plexAccountDisplayName = await _dbContext.GetPlexAccountDisplayName(plexAccountId, cancellationToken);

        // Retrieve all accessible servers for the PlexAccount
        var plexServers = await _dbContext.GetAccessiblePlexServers(plexAccountId, cancellationToken);
        if (plexServers.IsFailed)
            return plexServers.LogError();

        if (!plexServers.Value.Any())
            return Result.Ok();

        // Inspect all PlexServers

        await _mediator.Send(
            new QueueInspectPlexServerJobCommand(plexServers.Value.Select(x => x.Id).ToList()),
            CancellationToken.None
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
