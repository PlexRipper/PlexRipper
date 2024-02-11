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
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IPlexLibraryService _plexLibraryService;
    private readonly IPlexServerConnectionsService _plexServerConnectionsService;

    private readonly IPlexApiService _plexServiceApi;

    private readonly IServerSettingsModule _serverSettingsModule;
    private readonly ISyncServerScheduler _syncServerScheduler;
    private readonly IAddOrUpdatePlexServersCommand _addOrUpdatePlexServersCommand;
    private readonly IAddOrUpdatePlexAccountServersCommand _addOrUpdatePlexAccountServersCommand;

    #endregion

    #region Constructors

    public PlexServerService(
        ILog log,
        IMediator mediator,
        IPlexRipperDbContext dbContext,
        IPlexApiService plexServiceApi,
        IPlexLibraryService plexLibraryService,
        IServerSettingsModule serverSettingsModule,
        ISyncServerScheduler syncServerScheduler,
        IAddOrUpdatePlexServersCommand addOrUpdatePlexServersCommand,
        IAddOrUpdatePlexAccountServersCommand addOrUpdatePlexAccountServersCommand,
        IPlexServerConnectionsService plexServerConnectionsService)
    {
        _log = log;
        _mediator = mediator;
        _dbContext = dbContext;
        _serverSettingsModule = serverSettingsModule;
        _syncServerScheduler = syncServerScheduler;
        _addOrUpdatePlexServersCommand = addOrUpdatePlexServersCommand;
        _addOrUpdatePlexAccountServersCommand = addOrUpdatePlexAccountServersCommand;
        _plexServerConnectionsService = plexServerConnectionsService;
        _plexServiceApi = plexServiceApi;
        _plexLibraryService = plexLibraryService;
    }

    #endregion

    #region Methods

    #region Public

    public async Task<Result<PlexAccount>> ChoosePlexAccountToConnect(int plexServerId, CancellationToken ct = default)
    {
        if (plexServerId <= 0)
            return ResultExtensions.IsInvalidId(nameof(plexServerId), plexServerId);

        var plexAccountsResult = await _dbContext.GetPlexAccountsWithAccessAsync(plexServerId, ct);
        if (plexAccountsResult.IsFailed)
            return plexAccountsResult.ToResult();

        var plexAccounts = plexAccountsResult.Value.FindAll(x => x.IsEnabled);
        if (!Enumerable.Any<PlexAccount>(plexAccounts))
            return Result.Fail($"There are no enabled accounts that can access PlexServer with id: {plexServerId}").LogError();

        if (plexAccounts.Count == 1)
            return Result.Ok(Enumerable.First<PlexAccount>(plexAccounts));

        // Prefer to use a non-main account
        var dummyAccount = Enumerable.FirstOrDefault<PlexAccount>(plexAccounts, x => !x.IsMain);
        if (dummyAccount is not null)
            return Result.Ok(dummyAccount);

        var mainAccount = Enumerable.FirstOrDefault<PlexAccount>(plexAccounts, x => x.IsMain);
        if (mainAccount is not null)
            return Result.Ok(mainAccount);

        return Result.Fail($"No account could be chosen to connect to PlexServer with id: {plexServerId}").LogError();
    }

    #region CRUD

    public async Task<Result> SetPreferredConnection(int plexServerId, int plexServerConnectionId)
    {
        return await _mediator.Send(new SetPreferredPlexServerConnectionCommand(plexServerId, plexServerConnectionId));
    }

    #endregion

    #endregion

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
            var refreshResult = await _mediator.Send(new RefreshAccessiblePlexServersCommand(plexAccount.Id));
            if (refreshResult.IsFailed)
                return refreshResult.ToResult();

// TODO needs refresh libraries accessible
            // await _plexLibraryService.RetrieveAccessibleLibrariesAsync(plexAccountId,)
        }
        else
        {
            _log.Here()
                .Warning("Skipping refreshing of the accessible plex server in {NameOfInspectAllPlexServersByAccountId}",
                    nameof(InspectAllPlexServersByAccountId));
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
    public async Task<Result<PlexServer>> RefreshPlexServerConnectionsAsync(int plexServerId, CancellationToken ct = default)
    {
        // Pick an account that has access to the PlexServer to connect with
        var plexAccountResult = await ChoosePlexAccountToConnect(plexServerId, ct);
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
        var updateResult = await _addOrUpdatePlexServersCommand.ExecuteAsync(serverList);
        if (updateResult.IsFailed)
            return updateResult;

        // We only want to update tokens for the plexServer and discard the rest
        var plexAccountTokensResult = await _addOrUpdatePlexAccountServersCommand.ExecuteAsync(plexAccount.Id, serverAccessTokens);
        if (plexAccountTokensResult.IsFailed)
            return plexAccountTokensResult;

        return await _mediator.Send(new GetPlexServerByIdQuery(plexServerId, true));
    }

    #endregion
}