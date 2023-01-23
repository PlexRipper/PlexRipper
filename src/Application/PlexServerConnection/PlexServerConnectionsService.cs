using AutoMapper;
using Data.Contracts;

namespace PlexRipper.Application;

public class PlexServerConnectionsService : IPlexServerConnectionsService
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ISignalRService _signalRService;
    private readonly IPlexApiService _plexApiService;

    public PlexServerConnectionsService(IMediator mediator, IMapper mapper, ISignalRService signalRService, IPlexApiService plexApiService)
    {
        _mediator = mediator;
        _mapper = mapper;
        _signalRService = signalRService;
        _plexApiService = plexApiService;
    }

    #region CRUD

    public Task<Result<PlexServerConnection>> GetPlexServerConnectionAsync(int plexServerConnectionId)
    {
        return _mediator.Send(new GetPlexServerConnectionByIdQuery(plexServerConnectionId, includeStatus: true));
    }

    public async Task<Result<List<PlexServerConnection>>> GetAllPlexServerConnectionsAsync()
    {
        return await _mediator.Send(new GetAllPlexServerConnectionsQuery(includeStatus: true));
    }

    #endregion

    /// <inheritdoc />
    public async Task<Result> CheckAllConnectionsOfPlexServerAsync(int plexServerId)
    {
        var plexServerResult = await _mediator.Send(new GetPlexServerByIdQuery(plexServerId, true));
        if (plexServerResult.IsFailed)
            return plexServerResult.ToResult();

        // Create connection check tasks for all connections
        var connectionTasks = plexServerResult.Value
            .PlexServerConnections
            .Select(async plexServerConnection => await CheckPlexServerConnectionStatusAsync(plexServerConnection));

        var tasksResult = await Task.WhenAll(connectionTasks);
        Result.Merge(tasksResult);

        if (tasksResult.Any(statusResult => statusResult.Value.IsSuccessful))
            return Result.Ok();

        return Result.Fail($"All connections to plex server with id: {plexServerId} failed to connect").LogError();
    }

    public async Task<Result> CheckAllConnectionsOfPlexServersByAccountIdAsync(int plexAccountId)
    {
        var plexAccountResult = await _mediator.Send(new GetPlexAccountByIdQuery(plexAccountId, true));
        if (plexAccountResult.IsFailed)
        {
            return plexAccountResult
                .WithError($"Could not retrieve any PlexAccount from database with id {plexAccountId}.")
                .LogError();
        }

        var plexServers = plexAccountResult.Value.PlexServers;

        var serverTasks = plexServers.Select(async plexServer => await CheckAllConnectionsOfPlexServerAsync(plexServer.Id));

        var tasksResult = await Task.WhenAll(serverTasks);
        return Result.OkIf(tasksResult.Any(x => x.IsSuccess),
                $"None of the servers that were checked for {nameof(PlexAccount)} with id {plexAccountId} were connectable")
            .LogIfFailed();
    }

    /// <inheritdoc/>
    public async Task<Result<PlexServerStatus>> CheckPlexServerConnectionStatusAsync(
        int plexServerConnectionId,
        bool trimEntries = true)
    {
        var plexServerConnectionResult = await _mediator.Send(new GetPlexServerConnectionByIdQuery(plexServerConnectionId));
        if (plexServerConnectionResult.IsFailed)
            return plexServerConnectionResult.ToResult();

        return await CheckPlexServerConnectionStatusAsync(plexServerConnectionResult.Value, trimEntries);
    }

    public async Task<Result<PlexServerStatus>> CheckPlexServerConnectionStatusAsync(PlexServerConnection plexServerConnection, bool trimEntries = true)
    {
        // The call-back action from the httpClient
        async void Action(PlexApiClientProgress progress)
        {
            var checkStatusProgress = _mapper.Map<ServerConnectionCheckStatusProgress>(progress);
            checkStatusProgress.PlexServerConnection = plexServerConnection;
            await _signalRService.SendServerConnectionCheckStatusProgress(checkStatusProgress);
        }

        // Request status
        var serverStatusResult = await _plexApiService.GetPlexServerStatusAsync(plexServerConnection.Id, Action);
        if (serverStatusResult.IsFailed)
            return serverStatusResult.LogError();

        // Add plexServer status to DB, the PlexServerStatus table functions as a server log.
        var result = await _mediator.Send(new CreatePlexServerStatusCommand(serverStatusResult.Value));
        if (result.IsFailed)
            return result.ToResult();

        if (trimEntries)
        {
            // Ensure that there are not too many PlexServerStatuses stored.
            var trimResult = await _mediator.Send(new TrimPlexServerStatusCommand(serverStatusResult.Value.PlexServerId));
            if (trimResult.IsFailed)
                return trimResult.ToResult();
        }

        return serverStatusResult.Value;
    }


    /// <summary>
    /// Send server inspect status to front-end
    /// </summary>
    /// <param name="progress"></param>
    private async Task SendServerProgress(InspectServerProgress progress)
    {
        await _signalRService.SendServerInspectStatusProgress(progress);
    }
}