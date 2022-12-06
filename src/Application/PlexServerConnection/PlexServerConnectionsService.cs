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

    public async Task<Result<PlexServerStatus>> CheckPlexServerConnection(int plexServerConnectionId)
    {
        var plexServerConnectionResult = await _mediator.Send(new GetPlexServerConnectionByIdQuery(plexServerConnectionId));
        if (plexServerConnectionResult.IsFailed)
            return plexServerConnectionResult.ToResult();

        return await CheckPlexServerConnection(plexServerConnectionResult.Value);
    }

    public async Task<Result<PlexServerStatus>> CheckPlexServerConnection(PlexServerConnection plexServerConnection)
    {
        Log.Information($"Checking Plex server connection {plexServerConnection.Name} for connectivity");

        // The call-back action from the httpClient
        async void Action(PlexApiClientProgress progress)
        {
            var serverProgress = _mapper.Map<InspectServerProgress>(progress);
            serverProgress.PlexServerConnection = plexServerConnection;
            serverProgress.PlexServerId = plexServerConnection.PlexServerId;
            await SendServerProgress(serverProgress);
        }

        // Start with simple status request
        var serverStatusResult = await CheckPlexServerConnectionStatusAsync(plexServerConnection.Id, false);
        if (serverStatusResult.IsSuccess && serverStatusResult.Value.IsSuccessful)
        {
            await SendServerProgress(new InspectServerProgress
            {
                Completed = true,
                ConnectionSuccessful = true,
                StatusCode = serverStatusResult.Value.StatusCode,
                Message = "Server connection was successful!",
                PlexServerConnection = plexServerConnection,
            });
        }
        else
        {
            Log.Error($"Failed to retrieve the serverStatus for {plexServerConnection.Name} - {plexServerConnection.Url}");
            await SendServerProgress(new InspectServerProgress
            {
                Completed = true,
                ConnectionSuccessful = false,
                StatusCode = serverStatusResult.Value.StatusCode,
                Message = "Server connection failed!",
                PlexServerConnection = plexServerConnection,
            });
        }

        return serverStatusResult;
    }

    /// <inheritdoc/>
    public async Task<Result<PlexServerStatus>> CheckPlexServerConnectionStatusAsync(
        int plexServerConnectionId,
        bool trimEntries = true)
    {
        var plexServerConnectionResult = await _mediator.Send(new GetPlexServerConnectionByIdQuery(plexServerConnectionId));
        if (plexServerConnectionResult.IsFailed)
            return plexServerConnectionResult.ToResult();

        // The call-back action from the httpClient
        async void Action(PlexApiClientProgress progress)
        {
            var checkStatusProgress = _mapper.Map<ServerConnectionCheckStatusProgress>(progress);
            checkStatusProgress.PlexServerConnection = plexServerConnectionResult.Value;
            await _signalRService.SendServerConnectionCheckStatusProgress(checkStatusProgress);
        }

        // Request status
        var serverStatusResult = await _plexApiService.GetPlexServerStatusAsync(plexServerConnectionId, Action);
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