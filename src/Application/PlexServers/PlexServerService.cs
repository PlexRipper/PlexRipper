using Application.Contracts;
using BackgroundServices.Contracts;
using Data.Contracts;
using Logging.Interface;

namespace PlexRipper.Application;

public class PlexServerService : IPlexServerService
{
    #region Fields

    private readonly ILog _log;
    private readonly IMediator _mediator;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IPlexLibraryService _plexLibraryService;
    private readonly IPlexServerConnectionsService _plexServerConnectionsService;
    private readonly ISyncServerScheduler _syncServerScheduler;

    #endregion

    #region Constructors

    public PlexServerService(
        ILog log,
        IMediator mediator,
        IPlexRipperDbContext dbContext,
        IPlexLibraryService plexLibraryService,
        ISyncServerScheduler syncServerScheduler,
        IPlexServerConnectionsService plexServerConnectionsService)
    {
        _log = log;
        _mediator = mediator;
        _dbContext = dbContext;
        _syncServerScheduler = syncServerScheduler;
        _plexServerConnectionsService = plexServerConnectionsService;
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
        var refreshResult = await _mediator.Send(new RefreshPlexServerConnectionsCommand(plexServerId));
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
}