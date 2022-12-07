using AutoMapper;

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
    public async Task<Result<List<PlexServerStatus>>> CheckAllPlexServerConnectionsAsync(int plexServerId)
    {
        var plexServerResult = await _mediator.Send(new GetPlexServerByIdQuery(plexServerId, true));
        if (plexServerResult.IsFailed)
            return plexServerResult.ToResult();

        // Create connection check tasks for all connections
        var connectionTasks = plexServerResult.Value
            .PlexServerConnections
            .Select(async plexServerConnection => await CheckPlexServerConnectionStatusAsync(plexServerConnection));

        var tasksResult = await Task.WhenAll(connectionTasks);
        var x = Result.Merge(tasksResult);
        return Result.Ok(x.Value.ToList());
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
            return serverStatusResult;

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

        return await _mediator.Send(new GetPlexServerStatusByIdQuery(result.Value));
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