using AutoMapper;
using PlexRipper.Application.PlexAccounts;

namespace PlexRipper.Application;

public class PlexServerService : IPlexServerService
{
    private readonly IMapper _mapper;

    private readonly IMediator _mediator;

    private readonly IPlexLibraryService _plexLibraryService;

    private readonly ISignalRService _signalRService;

    private readonly IServerSettingsModule _serverSettingsModule;

    private readonly IPlexApiService _plexServiceApi;

    private readonly List<int> _currentSyncingPlexServers = new();

    public PlexServerService(
        IMapper mapper,
        IMediator mediator,
        IPlexApiService plexServiceApi,
        IPlexLibraryService plexLibraryService,
        ISignalRService signalRService,
        IServerSettingsModule serverSettingsModule)
    {
        _mapper = mapper;
        _mediator = mediator;
        _plexLibraryService = plexLibraryService;
        _signalRService = signalRService;
        _serverSettingsModule = serverSettingsModule;
        _plexServiceApi = plexServiceApi;
    }

    /// <inheritdoc/>
    public async Task<Result<List<PlexServer>>> RetrieveAccessiblePlexServersAsync(PlexAccount plexAccount)
    {
        if (plexAccount == null)
            return ResultExtensions.IsNull(nameof(plexAccount)).LogWarning();

        Log.Debug($"Refreshing Plex servers for PlexAccount: {plexAccount.Id}");
        var tupleResult = await _plexServiceApi.GetServersAsync(plexAccount);
        var serversResult = tupleResult.servers;
        var tokensResult = tupleResult.tokens;

        if (serversResult.IsFailed || tokensResult.IsFailed)
            return serversResult;

        if (!serversResult.Value.Any())
            return Result.Ok();

        var serverList = serversResult.Value;
        var serverAccessTokens = tokensResult.Value;

        // Add PlexServers and their PlexServerConnections
        var updateResult = await _mediator.Send(new AddOrUpdatePlexServersCommand(serverList));
        if (updateResult.IsFailed)
            return updateResult;

        // Check if every server has a settings entry
        _serverSettingsModule.EnsureAllServersHaveASettingsEntry(serverList);

        var plexAccountTokensResult = await _mediator.Send(new AddOrUpdatePlexAccountServersCommand(plexAccount, serverAccessTokens));
        if (plexAccountTokensResult.IsFailed)
            return plexAccountTokensResult;

        return await _mediator.Send(new GetAllPlexServersByPlexAccountIdQuery(plexAccount.Id));
    }

    public async Task<Result> SyncPlexServers(bool forceSync = false)
    {
        var plexServersResult = await GetAllPlexServersAsync(false);
        if (plexServersResult.IsFailed)
            return plexServersResult.ToResult();

        return await SyncPlexServers(plexServersResult.Value.Select(x => x.Id).ToList());
    }

    public async Task<Result> SyncPlexServers(List<int> plexServerIds, bool forceSync = false)
    {
        var results = new List<Result>();

        foreach (var plexServerId in plexServerIds)
        {
            var result = await SyncPlexServer(plexServerId, forceSync);
            if (result.IsFailed)
                results.Add(result);
        }

        if (results.Any())
        {
            var failedResult = Result.Fail("Some libraries failed to sync");
            results.ForEach(x => { failedResult.AddNestedErrors(x.Errors); });
            return failedResult.LogError();
        }

        return Result.Ok();
    }

    /// <inheritdoc/>
    public async Task<Result> SyncPlexServer(int plexServerId, bool forceSync = false)
    {
        if (_currentSyncingPlexServers.Contains(plexServerId))
            return Result.Ok($"PlexServer with id {plexServerId} is already syncing").LogWarning().ToResult();

        _currentSyncingPlexServers.Add(plexServerId);

        var plexServerResult = await _mediator.Send(new GetPlexServerByIdQuery(plexServerId, true));
        if (plexServerResult.IsFailed)
        {
            _currentSyncingPlexServers.Remove(plexServerId);
            return plexServerResult.ToResult();
        }

        var plexServer = plexServerResult.Value;
        var results = new List<Result>();

        var plexLibraries = forceSync
            ? plexServer.PlexLibraries
            : plexServer.PlexLibraries.FindAll(
                x => x.Outdated
                     && x.Type is PlexMediaType.Movie or PlexMediaType.TvShow);

        if (!plexLibraries.Any())
        {
            _currentSyncingPlexServers.Remove(plexServerId);
            return Result.Ok()
                .WithReason(new Success($"PlexServer {plexServer.Name} with id {plexServer.Id} has no libraries to sync"))
                .LogInformation();
        }

        // Send progress on every library update
        var progressList = new List<LibraryProgress>();

        // Initialize list
        plexLibraries.ForEach(x => progressList.Add(new LibraryProgress(x.Id, 0, x.MediaCount)));

        var progress = new Action<LibraryProgress>(libraryProgress =>
        {
            var i = progressList.FindIndex(x => x.Id == libraryProgress.Id);
            if (i != -1)
                progressList[i] = libraryProgress;
            else
                progressList.Add(libraryProgress);

            _signalRService.SendServerSyncProgressUpdate(new SyncServerProgress(plexServerId, progressList));
        });

        // Sync movie type libraries first because it is a lot quicker than TvShows.
        foreach (var library in plexLibraries.FindAll(x => x.Type == PlexMediaType.Movie))
        {
            var result = await _plexLibraryService.RefreshLibraryMediaAsync(library.Id, progress);
            if (result.IsFailed)
                results.Add(result.ToResult());
        }

        foreach (var library in plexLibraries.FindAll(x => x.Type == PlexMediaType.TvShow))
        {
            var result = await _plexLibraryService.RefreshLibraryMediaAsync(library.Id, progress);
            if (result.IsFailed)
                results.Add(result.ToResult());
        }

        if (results.Any())
        {
            var failedResult = Result.Fail($"Some libraries failed to sync in PlexServer: {plexServer.Name}");
            results.ForEach(x => { failedResult.AddNestedErrors(x.Errors); });
            _currentSyncingPlexServers.Remove(plexServerId);
            return failedResult.LogError();
        }

        _currentSyncingPlexServers.Remove(plexServerId);
        return Result.Ok();
    }

    /// <summary>
    /// Inspects the <see cref="PlexServer">PlexServers</see> for connectivity and attempts to fix those which return errors.
    /// When successfully connected, the <see cref="PlexLibrary">PlexLibraries</see> are stored in the database.
    /// </summary>
    /// <param name="plexAccountId"></param>
    /// <returns></returns>
    public async Task<Result> InspectPlexServers(int plexAccountId)
    {
        var plexAccountResult = await _mediator.Send(new GetPlexAccountByIdQuery(plexAccountId, true));
        if (plexAccountResult.IsFailed)
            return plexAccountResult.WithError($"Could not retrieve any PlexAccount from database with id {plexAccountId}.").LogError();

        var plexServers = plexAccountResult.Value.PlexServers;

        Log.Information($"Inspecting {plexServers.Count} PlexServers for PlexAccount: {plexAccountResult.Value.DisplayName}");

        // Create inspect tasks for all plexServers
        var tasks = plexServers.Select(async plexServer =>
        {
            var inspectResult = await InspectPlexServerConnections(plexServer);
            if (inspectResult.IsSuccess)
                await _plexLibraryService.RetrieveAccessibleLibrariesAsync(plexAccountResult.Value, plexServer);

            return inspectResult;
        });

        await Task.WhenAll(tasks);

        return await _mediator.Send(new UpdatePlexServersCommand(plexServers));
    }

    /// <summary>
    /// Checks every <see cref="PlexServerConnection"/> in parallel of a <see cref="PlexServer"/> whether it connects or not
    /// and then stores that <see cref="PlexServerStatus"/> in the database.
    /// </summary>
    /// <param name="plexServer">The <see cref="PlexServer"/> to check the connections for.</param>
    /// <returns></returns>
    public async Task<Result<List<PlexServerStatus>>> InspectPlexServerConnections(PlexServer plexServer)
    {
        // Send server inspect status to front-end
        async Task SendServerProgress(InspectServerProgress progress)
        {
            progress.PlexServerId = plexServer.Id;
            await _signalRService.SendServerInspectStatusProgress(progress);
        }

        await SendServerProgress(new InspectServerProgress
        {
            Message = $"Inspecting Server connections of {plexServer.Name}",
        });

        // Create inspect tasks for all plexServers
        var tasks = plexServer.PlexServerConnections.Select(async plexServerConnection =>
        {
            // The call-back action from the httpClient
            async void Action(PlexApiClientProgress progress)
            {
                var serverProgress = _mapper.Map<InspectServerProgress>(progress);
                serverProgress.PlexServerConnection = plexServerConnection;
                await SendServerProgress(serverProgress);
            }

            // Start with simple status request
            var serverStatusResult = await CheckPlexServerStatusAsync(plexServerConnection.Id, false, Action);
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
                Log.Error($"Failed to retrieve the serverStatus for {plexServer.Name} - {plexServerConnection.Url}");
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

            // TODO This might be obsolete with the new connections endpoint for each server
            // Apply possible fixes and try again
            // var dnsFixMsg = $"Attempting to DNS fix the connection with server {plexServer.Name}";
            // Log.Information(dnsFixMsg);
            // await SendServerProgress(new InspectServerProgress
            // {
            //     AttemptingApplyDNSFix = true,
            //     Message = dnsFixMsg,
            // });
            //
            // plexServer.ServerFixApplyDNSFix = true;
            // serverStatusResult = await CheckPlexServerStatusAsync(plexAccountId, false, action);
            //
            // if (serverStatusResult.IsSuccess && serverStatusResult.Value.IsSuccessful)
            // {
            //     // DNS fix worked
            //     dnsFixMsg = $"Server DNS Fix worked on {plexServer.Name}, connection successful!";
            //     Log.Information(dnsFixMsg);
            //     await SendServerProgress(new InspectServerProgress
            //     {
            //         Message = dnsFixMsg,
            //         Completed = true,
            //         ConnectionSuccessful = true,
            //         AttemptingApplyDNSFix = true,
            //     });
            //     return Result.Ok(plexServer);
            // }
            //
            // // DNS fix did not work
            // dnsFixMsg = $"Server DNS Fix did not help with server {plexServer.Name} - {plexServer.GetServerUrl()}";
            // Log.Warning(dnsFixMsg);
            // await SendServerProgress(new InspectServerProgress
            // {
            //     AttemptingApplyDNSFix = true,
            //     Completed = true,
            //     Message = dnsFixMsg,
            // });
        });

        var tasks2 = await Task.WhenAll(tasks);
        return Result.Merge(tasks2).ToResult();
    }

    public async Task<Result<PlexServer>> InspectPlexServerConnections(int plexServerId)
    {
        var plexServerResult = await _mediator.Send(new GetPlexServerByIdQuery(plexServerId));
        if (plexServerResult.IsFailed)
            return plexServerResult.WithError($"Could not retrieve any PlexServer from database with id {plexServerId}.").LogError();

        await InspectPlexServerConnections(plexServerResult.Value);

        _serverSettingsModule.EnsureAllServersHaveASettingsEntry(new List<PlexServer> { plexServerResult.Value });

        return plexServerResult;
    }

    /// <summary>
    /// Check if the <see cref="PlexServer"/> is available and log the status.
    /// </summary>
    /// <param name="plexServerConnectionId"></param>
    /// <param name="trimEntries">Delete entries which are older than a certain threshold.</param>
    /// <param name="progressAction">Optional callback to report progress on checking the PlexServerStatus</param>
    /// <returns>The latest <see cref="PlexServerStatus"/>.</returns>
    public async Task<Result<PlexServerStatus>> CheckPlexServerStatusAsync(
        int plexServerConnectionId,
        bool trimEntries = true,
        Action<PlexApiClientProgress> progressAction = null)
    {
        // Request status
        var serverStatusResult = await _plexServiceApi.GetPlexServerStatusAsync(plexServerConnectionId, progressAction);
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

    public Task<Result> RemoveInaccessibleServers()
    {
        var result = _mediator.Send(new RemoveInaccessibleServersCommand());
        return result;
    }

    #region CRUD

    public Task<Result<PlexServer>> GetServerAsync(int plexServerId)
    {
        return _mediator.Send(new GetPlexServerByIdQuery(plexServerId, true));
    }

    /// <inheritdoc/>
    public async Task<Result<List<PlexServer>>> GetAllPlexServersAsync(bool includeLibraries, int plexAccountId = 0)
    {
        // Retrieve all servers
        return await _mediator.Send(new GetAllPlexServersQuery(includeLibraries, plexAccountId));
    }

    #endregion
}