using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.Logging;
using Serilog;

namespace Reaparr.Application;

/// <summary>
/// Checks every <see cref="PlexServerConnection"/> in parallel of a <see cref="PlexServer"/> whether it connects or not
/// and then stores that <see cref="PlexServerStatus"/> in the database.
/// </summary>
/// <param name="PlexServerId">The id of the <see cref="PlexServer" /> to check the connections for.</param>
/// <returns>Returns successful result if any connection connected.</returns>
public record CheckAllConnectionsStatusByPlexServerCommand(int PlexServerId) : ICommand<Result<List<PlexServerStatus>>>;

public class CheckAllConnectionsStatusByPlexServerValidator
    : AbstractValidator<CheckAllConnectionsStatusByPlexServerCommand>
{
    public CheckAllConnectionsStatusByPlexServerValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class CheckAllConnectionsStatusByPlexServerHandler
    : ICommandHandler<CheckAllConnectionsStatusByPlexServerCommand, Result<List<PlexServerStatus>>>
{
    private readonly Serilog.ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IEventPublisher _eventPublisher;
    private readonly ISignalRService _signalRService;

    public CheckAllConnectionsStatusByPlexServerHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor,
        IEventPublisher eventPublisher,
        ISignalRService signalRService
    )
    {
        _log = log.ForContext<CheckAllConnectionsStatusByPlexServerHandler>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
        _eventPublisher = eventPublisher;
        _signalRService = signalRService;
    }

    public async Task<Result<List<PlexServerStatus>>> ExecuteAsync(
        CheckAllConnectionsStatusByPlexServerCommand command,
        CancellationToken cancellationToken
    )
    {
        var plexServerId = command.PlexServerId;

        var plexServer = await _dbContext
            .PlexServers.Include(x => x.PlexServerConnections)
            .GetAsync(plexServerId, cancellationToken);

        if (plexServer == null)
            return ResultExtensions.EntityNotFound(nameof(plexServerId), plexServerId).LogError();

        var plexServerName = await _dbContext.GetPlexServerNameById(plexServerId, cancellationToken);
        if (!plexServer.IsEnabled)
        {
            return ResultExtensions
                .ServerIsNotEnabled(plexServerName, plexServerId, nameof(CheckAllConnectionsStatusByPlexServerCommand))
                .LogError();
        }

        var connections = plexServer.PlexServerConnections.ToList();
        if (!connections.Any())
        {
            return _log.Here().ErrorResult("No connections found for the plex server {PlexServerName}", plexServerName);
        }

        var previousResult = await _dbContext.IsServerOnline(plexServerId, cancellationToken: cancellationToken);

        // Create connection check tasks for all connections
        var connectionTasks = connections.Select(async plexServerConnection =>
            await _commandExecutor.Send(
                new CheckConnectionStatusByIdCommand(plexServerConnection.Id),
                cancellationToken
            )
        );

        var tasksResult = await Task.WhenAll(connectionTasks);
        var combinedResults = Result.Merge(tasksResult);

        await _signalRService.SendRefreshNotificationAsync([RefreshDataType.PlexServerConnection], cancellationToken);

        // Compare previous and current online status
        var currentOnlineStatus = tasksResult.Any(statusResult => statusResult.ValueOrDefault?.IsSuccessful != null);

        if (previousResult != currentOnlineStatus)
        {
            await _eventPublisher.PublishAsync(
                new ServerOnlineStatusChangedNotification(plexServerId, currentOnlineStatus),
                CancellationToken.None
            );
        }

        if (currentOnlineStatus)
            return Result.Ok(combinedResults.Value.ToList());

        return _log.Here()
            .ErrorResult(
                "All connections to plex server with name: {PlexServerName} and id: {PlexServerId} failed to connect",
                plexServerName,
                plexServerId
            );
    }
}
