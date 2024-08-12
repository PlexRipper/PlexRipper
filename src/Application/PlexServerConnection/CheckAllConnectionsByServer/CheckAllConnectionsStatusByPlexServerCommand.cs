using Data.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

/// <summary>
/// Checks every <see cref="PlexServerConnection"/> in parallel of a <see cref="PlexServer"/> whether it connects or not
/// and then stores that <see cref="PlexServerStatus"/> in the database.
/// </summary>
/// <param name="PlexServerId">The id of the <see cref="PlexServer" /> to check the connections for.</param>
/// <returns>Returns successful result if any connection connected.</returns>
public record CheckAllConnectionsStatusByPlexServerCommand(int PlexServerId) : IRequest<Result<List<PlexServerStatus>>>;

public class CheckAllConnectionsStatusByPlexServerCommandValidator
    : AbstractValidator<CheckAllConnectionsStatusByPlexServerCommand>
{
    public CheckAllConnectionsStatusByPlexServerCommandValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class CheckAllConnectionsStatusByPlexServerCommandHandler
    : IRequestHandler<CheckAllConnectionsStatusByPlexServerCommand, Result<List<PlexServerStatus>>>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;

    public CheckAllConnectionsStatusByPlexServerCommandHandler(IPlexRipperDbContext dbContext, IMediator mediator)
    {
        _dbContext = dbContext;
        _mediator = mediator;
    }

    public async Task<Result<List<PlexServerStatus>>> Handle(
        CheckAllConnectionsStatusByPlexServerCommand command,
        CancellationToken cancellationToken
    )
    {
        var plexServerId = command.PlexServerId;

        var plexServer = await _dbContext.PlexServers.GetAsync(plexServerId, cancellationToken);

        if (plexServer == null)
            return ResultExtensions.EntityNotFound(nameof(plexServerId), plexServerId).LogError();

        if (!plexServer.IsEnabled)
        {
            return ResultExtensions
                .ServerIsNotEnabled(plexServer.Name, plexServerId, nameof(CheckAllConnectionsStatusByPlexServerCommand))
                .LogError();
        }

        var connections = await _dbContext
            .PlexServerConnections.Where(x => x.PlexServerId == plexServerId)
            .ToListAsync(cancellationToken);

        if (!connections.Any())
            return Result.Fail($"No connections found for the given plex server id {plexServerId}").LogError();

        // Create connection check tasks for all connections
        var connectionTasks = connections.Select(async plexServerConnection =>
            await _mediator.Send(new CheckConnectionStatusByIdCommand(plexServerConnection.Id), cancellationToken)
        );

        var tasksResult = await Task.WhenAll(connectionTasks);
        var combinedResults = Result.Merge(tasksResult);

        if (tasksResult.Any(statusResult => statusResult.Value.IsSuccessful))
            return Result.Ok(combinedResults.Value.ToList());

        return Result.Fail($"All connections to plex server with id: {plexServerId} failed to connect").LogError();
    }
}
