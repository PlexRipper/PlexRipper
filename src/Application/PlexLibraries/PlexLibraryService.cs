using Application.Contracts;
using Data.Contracts;
using Logging.Interface;
using PlexApi.Contracts;

namespace PlexRipper.Application;

public class PlexLibraryService : IPlexLibraryService
{
    #region Fields

    private readonly ILog _log;
    private readonly IMediator _mediator;

    private readonly IPlexApiService _plexServiceApi;
    private readonly IPlexRipperDbContext _dbContext;

    #endregion

    #region Constructors

    public PlexLibraryService(
        ILog log,
        IMediator mediator,
        IPlexApiService plexServiceApi,
        IPlexRipperDbContext dbContext)
    {
        _log = log;
        _mediator = mediator;
        _plexServiceApi = plexServiceApi;
        _dbContext = dbContext;
    }

    #endregion

    #region Methods

    #region Public

    /// <inheritdoc/>
    public async Task<Result<PlexLibrary>> GetPlexLibraryAsync(int libraryId)
    {
        var libraryDB = await _mediator.Send(new GetPlexLibraryByIdQuery(libraryId));

        if (libraryDB.IsFailed)
            return libraryDB;

        if (!libraryDB.Value.HasMedia)
        {
            _log.Information("PlexLibrary with id {LibraryId} has no media, forcing refresh from the PlexApi", libraryId);

            var refreshResult = await _mediator.Send(new RefreshLibraryMediaCommand(libraryId));
            if (refreshResult.IsFailed)
                return refreshResult.ToResult();
        }

        return await _mediator.Send(new GetPlexLibraryByIdQuery(libraryId));
    }

    #endregion

    #endregion

    #region RefreshLibrary

    public async Task<Result> RetrieveAccessibleLibrariesForAllAccountsAsync(int plexServerId)
    {
        if (plexServerId <= 0)
            return ResultExtensions.IsInvalidId(nameof(plexServerId)).LogWarning();

        var accountsResult = await _dbContext.GetPlexAccountsWithAccessAsync(plexServerId);
        if (accountsResult.IsFailed)
            return accountsResult.ToResult();

        foreach (var plexAccount in accountsResult.Value)
            await RetrieveAccessibleLibrariesAsync(plexAccount.Id, plexServerId);

        return Result.Ok();
    }

    /// <inheritdoc/>
    public async Task<Result> RetrieveAccessibleLibrariesAsync(int plexAccountId, int plexServerId)
    {
        if (plexServerId <= 0)
            return ResultExtensions.IsInvalidId(nameof(plexServerId), plexServerId).LogWarning();

        if (plexAccountId <= 0)
            return ResultExtensions.IsInvalidId(nameof(plexAccountId), plexAccountId).LogWarning();

        _log.Debug("Retrieving accessible PlexLibraries for plexServer with id: {PlexServerId} by using Plex account with id {PlexAccountId}", plexServerId,
            plexAccountId);

        var libraries = await _plexServiceApi.GetLibrarySectionsAsync(plexServerId, plexAccountId);
        if (libraries.IsFailed)
            return libraries.ToResult();

        if (!libraries.Value.Any())
        {
            var msg = $"{nameof(PlexServer)} with Id {plexServerId} returned no Plex libraries for Plex account with id {plexAccountId}";
            return Result.Fail(msg).LogWarning();
        }

        return await _mediator.Send(new AddOrUpdatePlexLibrariesCommand(plexAccountId, libraries.Value));
    }

    public async Task<Result> RetrieveAllAccessibleLibrariesAsync(int plexAccountId)
    {
        _log.Information("Retrieving accessible Plex libraries for Plex account with id {PlexAccountId}", plexAccountId);

        // TODO Replace with:
        // return await _dbContext.GetAllPlexServersByPlexAccountIdQuery(_mapper, plexAccountId, cancellationToken);

        var plexServersResult = await _mediator.Send(new GetAllPlexServersByPlexAccountIdQuery(plexAccountId));
        if (plexServersResult.IsFailed)
            return plexServersResult.ToResult().LogError();

        //var onlineServers = plexServersResult.Value.FindAll(x => x.)

        // Create connection check tasks for all connections
        var retrieveTasks = plexServersResult.Value
            .Select(async plexServer => await RetrieveAccessibleLibrariesAsync(plexAccountId, plexServer.Id));

        var tasksResult = await Task.WhenAll(retrieveTasks);
        return tasksResult.Merge();
    }

    #endregion
}