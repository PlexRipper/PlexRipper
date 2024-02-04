using Application.Contracts;
using BackgroundServices.Contracts;
using Data.Contracts;
using Logging.Interface;
using PlexApi.Contracts;
using Settings.Contracts;

namespace PlexRipper.Application;

public class PlexServerService : IPlexServerService
{
    #region Fields

    private readonly ILog _log;
    private readonly IMediator _mediator;
    private readonly IPlexLibraryService _plexLibraryService;
    private readonly IPlexServerConnectionsService _plexServerConnectionsService;
    private readonly IPlexServerRepository _plexServerRepository;

    private readonly IPlexApiService _plexServiceApi;

    private readonly IServerSettingsModule _serverSettingsModule;
    private readonly ISyncServerScheduler _syncServerScheduler;

    #endregion

    #region Constructors

    public PlexServerService(
        ILog log,
        IMediator mediator,
        IPlexApiService plexServiceApi,
        IPlexLibraryService plexLibraryService,
        IServerSettingsModule serverSettingsModule,
        ISyncServerScheduler syncServerScheduler,
        IPlexServerConnectionsService plexServerConnectionsService,
        IPlexServerRepository plexServerRepository)
    {
        _log = log;
        _mediator = mediator;
        _serverSettingsModule = serverSettingsModule;
        _syncServerScheduler = syncServerScheduler;
        _plexServerConnectionsService = plexServerConnectionsService;
        _plexServerRepository = plexServerRepository;
        _plexServiceApi = plexServiceApi;
        _plexLibraryService = plexLibraryService;
    }

    #endregion

    #region Methods

 #endregion

    #region InspectPlexServers

    /// <summary>
    /// Inspects the <see cref="PlexServer">PlexServers</see> for connectivity.
    /// When successfully connected, the <see cref="PlexLibrary">PlexLibraries</see> are stored in the database.
    /// </summary>
    /// <param name="plexAccountId"></param>
    /// <param name="skipRefreshAccessibleServers"></param>
    /// <returns></returns>
    public async Task<Result> InspectAllPlexServersByAccountId(int plexAccountId, bool skipRefreshAccessibleServers = false)
    {
        var plexAccountResult = await _mediator.Send(new GetPlexAccountByIdQuery(plexAccountId, true));
        if (plexAccountResult.IsFailed)
            return plexAccountResult.WithError($"Could not retrieve any PlexAccount from database with id {plexAccountId}.").LogError();

        var plexAccount = plexAccountResult.Value;
        var plexServers = plexAccountResult.Value.PlexServers;

        _log.Here()
            .Information("Inspecting {PlexServersCount} PlexServers for PlexAccount: {PlexAccountDisplayName}", plexServers.Count,
                plexAccountResult.Value.DisplayName);
        if (!skipRefreshAccessibleServers)
        {
            var refreshResult = await RefreshAccessiblePlexServersAsync(plexAccount.Id);
            if (refreshResult.IsFailed)
                return refreshResult.ToResult();

// TODO needs refresh libraries accessible
            // await _plexLibraryService.RetrieveAccessibleLibrariesAsync(plexAccountId,)
        }
        else
        {
            _log.Here()
                .Warning("Skipping {NameOfRefreshAccessiblePlexServersAsync} in {NameOfInspectAllPlexServersByAccountId}",
                    nameof(RefreshAccessiblePlexServersAsync), nameof(InspectAllPlexServersByAccountId));
        }

        // Create connection check tasks for all connections
        var checkResult = await _plexServerConnectionsService.CheckAllConnectionsOfPlexServersByAccountIdAsync(plexAccount.Id);
        if (checkResult.IsFailed)
            return checkResult;

        _log.Information("Successfully finished the inspection of all plexServers related to {NameOfPlexAccount} {PlexAccountId}", nameof(PlexAccount),
            plexAccountId);
        return Result.Ok();
    }

    public async Task<Result<PlexServer>> InspectPlexServer(int plexServerId)
    {
        var refreshResult = await RefreshPlexServerConnectionsAsync(plexServerId);
        if (refreshResult.IsFailed)
            return refreshResult.ToResult();

        var checkResult = await _plexServerConnectionsService.CheckAllConnectionsOfPlexServerAsync(plexServerId);
        if (checkResult.IsFailed)
            return checkResult.ToResult();

        await _plexLibraryService.RetrieveAccessibleLibrariesForAllAccountsAsync(plexServerId);

        await _syncServerScheduler.QueueSyncPlexServerJob(plexServerId, true);

        _log.Information("Successfully finished the inspection of {NameOfPlexServer} with id {PlexServerId}", nameof(PlexServer), plexServerId);
        return await _mediator.Send(new GetPlexServerByIdQuery(plexServerId, true));
    }

    #endregion

    #region RefreshPlexServers

    /// <inheritdoc/>
    public async Task<Result<PlexServer>> RefreshPlexServerConnectionsAsync(int plexServerId, CancellationToken token = default)
    {
        // Pick an account that has access to the PlexServer to connect with
        var plexAccountResult = await _plexServerRepository.ChoosePlexAccountToConnect(plexServerId, token);
        if (plexAccountResult.IsFailed)
            return plexAccountResult.ToResult();

        var plexServerResult = await _mediator.Send(new GetPlexServerByIdQuery(plexServerId));
        if (plexServerResult.IsFailed)
            return plexServerResult.ToResult();

        var plexServer = plexServerResult.Value;
        var plexAccount = plexAccountResult.Value;

        // Retrieve the PlexApi server data
        var tupleResult = await _plexServiceApi.GetAccessiblePlexServersAsync(plexAccount.Id);
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
        var plexAccountTokensResult = await _mediator.Send(new AddOrUpdatePlexAccountServersCommand(plexAccount, serverAccessTokens));
        if (plexAccountTokensResult.IsFailed)
            return plexAccountTokensResult;

        return await _mediator.Send(new GetPlexServerByIdQuery(plexServerId, true));
    }

    /// <inheritdoc/>
    public async Task<Result<List<PlexServer>>> RefreshAccessiblePlexServersAsync(int plexAccountId)
    {
        if (plexAccountId <= 0)
            return ResultExtensions.IsInvalidId(nameof(plexAccountId)).LogWarning();

        _log.Debug("Refreshing Plex servers for PlexAccount: {PlexAccountId}", plexAccountId);
        var tupleResult = await _plexServiceApi.GetAccessiblePlexServersAsync(plexAccountId);
        var serversResult = tupleResult.servers;
        var tokensResult = tupleResult.tokens;

        if (serversResult.IsFailed || tokensResult.IsFailed)
            return serversResult;

        if (!serversResult.Value.Any())
            return Result.Ok(new List<PlexServer>());

        var serverList = serversResult.Value;
        var serverAccessTokens = tokensResult.Value;

        // Add PlexServers and their PlexServerConnections
        var updateResult = await _mediator.Send(new AddOrUpdatePlexServersCommand(serverList));
        if (updateResult.IsFailed)
            return updateResult;

        // Check if every server has a settings entry
        _serverSettingsModule.EnsureAllServersHaveASettingsEntry(serverList);

        var plexAccountResult = await _mediator.Send(new GetPlexAccountByIdQuery(plexAccountId));
        if (plexAccountResult.IsFailed)
            return plexAccountResult.ToResult();

        var plexAccountTokensResult = await _mediator.Send(new AddOrUpdatePlexAccountServersCommand(plexAccountResult.Value, serverAccessTokens));
        if (plexAccountTokensResult.IsFailed)
            return plexAccountTokensResult;

        _log.Information("Successfully refreshed accessible Plex servers for account {PlexAccountDisplayName}", plexAccountResult.Value.DisplayName);
        return await _mediator.Send(new GetAllPlexServersByPlexAccountIdQuery(plexAccountId));
    }

    #endregion
}