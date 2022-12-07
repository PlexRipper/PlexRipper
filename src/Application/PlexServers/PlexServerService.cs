using PlexRipper.Application.PlexAccounts;

namespace PlexRipper.Application;

public class PlexServerService : IPlexServerService
{
    private readonly IMediator _mediator;

    private readonly IPlexLibraryService _plexLibraryService;

    private readonly ISignalRService _signalRService;

    private readonly IServerSettingsModule _serverSettingsModule;
    private readonly IPlexServerConnectionsService _plexServerConnectionsService;
    private readonly IPlexAccountService _accountService;

    private readonly IPlexApiService _plexServiceApi;

    private readonly List<int> _currentSyncingPlexServers = new();

    public PlexServerService(
        IMediator mediator,
        IPlexApiService plexServiceApi,
        IPlexLibraryService plexLibraryService,
        ISignalRService signalRService,
        IServerSettingsModule serverSettingsModule,
        IPlexServerConnectionsService plexServerConnectionsService,
        IPlexAccountService accountService)
    {
        _mediator = mediator;
        _plexLibraryService = plexLibraryService;
        _signalRService = signalRService;
        _serverSettingsModule = serverSettingsModule;
        _plexServerConnectionsService = plexServerConnectionsService;
        _accountService = accountService;
        _plexServiceApi = plexServiceApi;
    }


    #region InspectPlexServers

    /// <summary>
    /// Inspects the <see cref="PlexServer">PlexServers</see> for connectivity.
    /// When successfully connected, the <see cref="PlexLibrary">PlexLibraries</see> are stored in the database.
    /// </summary>
    /// <param name="plexAccountId"></param>
    /// <returns></returns>
    public async Task<Result> InspectPlexServers(int plexAccountId)
    {
        var plexAccountResult = await _mediator.Send(new GetPlexAccountByIdQuery(plexAccountId, true));
        if (plexAccountResult.IsFailed)
            return plexAccountResult.WithError($"Could not retrieve any PlexAccount from database with id {plexAccountId}.").LogError();

        var plexAccount = plexAccountResult.Value;
        var plexServers = plexAccountResult.Value.PlexServers;

        Log.Information($"Inspecting {plexServers.Count} PlexServers for PlexAccount: {plexAccountResult.Value.DisplayName}");

        var refreshResult = await RefreshAccessiblePlexServersAsync(plexAccount);
        if (refreshResult.IsFailed)
            return refreshResult.ToResult();

        var checkResult = await CheckAllServersWithAllConnections(plexServers);
        if (checkResult.IsFailed)
            return checkResult;

        return Result.Ok();
    }

    public async Task<Result<PlexServer>> InspectPlexServer(int plexServerId)
    {
        var refreshResult = await RefreshAccessiblePlexServerAsync(plexServerId);
        if (refreshResult.IsFailed)
            return refreshResult.ToResult();

        var checkResult = await _plexServerConnectionsService.CheckAllPlexServerConnectionsAsync(plexServerId);
        if (checkResult.IsFailed)
            return checkResult.ToResult();

        //TODO Add libraries syncing here

        return await _mediator.Send(new GetPlexServerByIdQuery(plexServerId, true));
    }

    #endregion

    #region RefreshPlexServers

    public async Task<Result<PlexServer>> RefreshAccessiblePlexServerAsync(int plexServerId)
    {
        // Pick an account that has access to the PlexServer to connect with
        var plexAccountResult = await _accountService.ChoosePlexAccountToConnect(plexServerId);
        if (plexAccountResult.IsFailed)
            return plexAccountResult.ToResult();

        var plexServerResult = await _mediator.Send(new GetPlexServerByIdQuery(plexServerId));
        if (plexServerResult.IsFailed)
            return plexServerResult.ToResult();

        var plexServer = plexServerResult.Value;

        // Retrieve the PlexApi server data
        var tupleResult = await _plexServiceApi.GetServersAsync(plexAccountResult.Value);
        var serverList = tupleResult.servers.Value
            .FindAll(x => x.MachineIdentifier == plexServer.MachineIdentifier);

        // Check if we got the plex server we are looking for
        if (!serverList.Any())
            return Result.Fail($"Could not retrieve the Plex server data with machine id: {plexServer.MachineIdentifier}");

        var serverAccessTokens = tupleResult.tokens.Value
            .FindAll(x => x.MachineIdentifier == plexServer.MachineIdentifier);

        // We only want to update one plexServer and discard the rest
        var updateResult = await _mediator.Send(new AddOrUpdatePlexServersCommand(serverList));
        if (updateResult.IsFailed)
            return updateResult;

        // We only want to update tokens for the plexServer and discard the rest
        var plexAccountTokensResult = await _mediator.Send(new AddOrUpdatePlexAccountServersCommand(plexAccountResult.Value, serverAccessTokens));
        if (plexAccountTokensResult.IsFailed)
            return plexAccountTokensResult;

        return await _mediator.Send(new GetPlexServerByIdQuery(plexServerId, true));
    }

    /// <inheritdoc/>
    public async Task<Result<List<PlexServer>>> RefreshAccessiblePlexServersAsync(PlexAccount plexAccount)
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

    #endregion

    #region SyncServers

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

        var plexServerResult = await _mediator.Send(new GetPlexServerByIdQuery(plexServerId, includeLibraries: true));
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

    #endregion

    public async Task<Result> CheckAllServersWithAllConnections(List<PlexServer> plexServers)
    {
        // Create connection check tasks for all connections
        var connectionTasks = plexServers
            .Select(async plexServer => await _plexServerConnectionsService.CheckAllPlexServerConnectionsAsync(plexServer.Id));

        var tasksResult = await Task.WhenAll(connectionTasks);
        return Result.Merge(tasksResult).ToResult();
    }

    #region CRUD

    public Task<Result<PlexServer>> GetServerAsync(int plexServerId)
    {
        return _mediator.Send(new GetPlexServerByIdQuery(plexServerId, includeLibraries: true));
    }

    /// <inheritdoc/>
    public async Task<Result<List<PlexServer>>> GetAllPlexServersAsync(bool includeLibraries, int plexAccountId = 0)
    {
        // Retrieve all servers
        return await _mediator.Send(new GetAllPlexServersQuery(includeLibraries, plexAccountId));
    }

    #endregion
}