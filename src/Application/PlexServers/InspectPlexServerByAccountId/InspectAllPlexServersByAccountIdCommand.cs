using FastEndpoints;
using FluentValidation;
using Reaparr.Data.Contracts;
using Reaparr.Logging;
using Serilog;

namespace Reaparr.Application;

/// <summary>
/// Inspects the <see cref="PlexServer">PlexServers</see> for connectivity.
/// When successfully connected, the <see cref="PlexLibrary">PlexLibraries</see> are stored in the database.
/// </summary>
/// <param name="PlexAccountId">The id of the <see cref="PlexAccount"/> to check for.</param>
/// <returns></returns>
public record InspectAllPlexServersByAccountIdCommand(int PlexAccountId) : ICommand<Result>;

public class InspectAllPlexServersByAccountIdCommandValidator
    : AbstractValidator<InspectAllPlexServersByAccountIdCommand>
{
    public InspectAllPlexServersByAccountIdCommandValidator()
    {
        RuleFor(x => x.PlexAccountId).GreaterThan(0);
    }
}

public class InspectAllPlexServersByAccountIdCommandHandler
    : ICommandHandler<InspectAllPlexServersByAccountIdCommand, Result>
{
    private readonly Serilog.ILogger _log;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IReaparrDbContext _dbContext;

    public InspectAllPlexServersByAccountIdCommandHandler(
        ILogger log,
        ICommandExecutor commandExecutor,
        IReaparrDbContext dbContext
    )
    {
        _log = log.ForContext<InspectAllPlexServersByAccountIdCommandHandler>();
        _commandExecutor = commandExecutor;
        _dbContext = dbContext;
    }

    public async Task<Result> ExecuteAsync(
        InspectAllPlexServersByAccountIdCommand command,
        CancellationToken cancellationToken
    )
    {
        var plexAccountId = command.PlexAccountId;
        var plexAccountDisplayName = await _dbContext.GetPlexAccountDisplayName(plexAccountId, cancellationToken);

        _log.Here()
            .Information(
                "Executing {MethodName} for Plex account: {PlexAccountName}",
                nameof(InspectAllPlexServersByAccountIdCommand),
                plexAccountDisplayName
            );

        var refreshResult = await _commandExecutor.Send(
            new RefreshPlexServerAccessCommand(plexAccountId),
            cancellationToken
        );
        if (refreshResult.IsFailed)
            return refreshResult.LogError();

        // Retrieve all accessible servers for the PlexAccount
        var plexServers = await _dbContext.GetAccessiblePlexServers(plexAccountId, cancellationToken);
        if (plexServers.IsFailed)
            return plexServers.LogError();

        if (!plexServers.Value.Any())
            return Result.Ok();

        // Inspect all PlexServers
        await _commandExecutor.Send(
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
