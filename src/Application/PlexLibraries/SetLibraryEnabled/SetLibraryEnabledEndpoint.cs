namespace Reaparr.Application;

public record SetLibraryEnabledRequest
{
    [RouteParam, BindFrom("PlexLibraryId")]
    public int PlexLibraryId { get; init; }

    public bool IsEnabled { get; init; } = true;
}

public class SetLibraryEnabledRequestValidator : Validator<SetLibraryEnabledRequest>
{
    public SetLibraryEnabledRequestValidator()
    {
        RuleFor(x => x.PlexLibraryId).GreaterThan(0);
    }
}

public class SetLibraryEnabledEndpoint : Endpoint<SetLibraryEnabledRequest, ResultDTO<PlexLibraryDTO>>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IMediaQueryCache _mediaQueryCache;
    private readonly ICommandExecutor _commandExecutor;
    private readonly INotificationHubService _notificationHubService;

    public SetLibraryEnabledEndpoint(
        ILogger log,
        IReaparrDbContext dbContext,
        IMediaQueryCache mediaQueryCache,
        ICommandExecutor commandExecutor,
        INotificationHubService notificationHubService
    )
    {
        _log = log.ForContext<SetLibraryEnabledEndpoint>();
        _dbContext = dbContext;
        _mediaQueryCache = mediaQueryCache;
        _commandExecutor = commandExecutor;
        _notificationHubService = notificationHubService;
    }

    public override void Configure()
    {
        Put(ApiRoutes.PlexLibraryController + "/{PlexLibraryId}/set-library-enabled");

        Description(x =>
            x.Accepts<SetLibraryEnabledRequest>()
                .Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexLibraryDTO>))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(SetLibraryEnabledRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);

        var plexLibrary = await _dbContext.PlexLibraries
            .AsTracking()
            .IgnoreIsEnabledFilter()
            .FirstOrDefaultAsync(x => x.Id == req.PlexLibraryId, ct);

        if (plexLibrary is null)
        {
            await Send.FluentResult(ResultExtensions.EntityNotFound(nameof(PlexLibrary), req.PlexLibraryId), ct);
            return;
        }

        if (req.IsEnabled)
        {
            await EnableLibraryAsync(plexLibrary, ct);
        }
        else
        {
            await DisableLibraryAsync(plexLibrary, ct);
        }

        // Re-fetch with ignore filter to return updated entity including IsEnabled
        var updatedLibrary = await _dbContext.PlexLibraries
            .IgnoreIsEnabledFilter()
            .FirstOrDefaultAsync(x => x.Id == req.PlexLibraryId, ct);

        if (updatedLibrary is null)
        {
            await Send.FluentResult(ResultExtensions.EntityNotFound(nameof(PlexLibrary), req.PlexLibraryId), ct);
            return;
        }

        await Send.FluentResult(Result.Ok(updatedLibrary), x => x.ToDTO(), ct);
    }

    private async Task EnableLibraryAsync(PlexLibrary plexLibrary, CancellationToken ct)
    {
        _log.Here().Information("Enabling PlexLibrary {PlexLibraryId}", plexLibrary.Id);

        plexLibrary.IsEnabled = true;
        await _dbContext.SaveChangesNewAsync(ct);

        // Queue a fresh sync
        await _commandExecutor.Send(new QueueLibrarySyncJobCommand([plexLibrary.Id]), ct);

        // Invalidate/re-build media query cache scopes for this library
        _mediaQueryCache.InvalidateLibrary(plexLibrary.Id, "PlexLibrary re-enabled");

        // Notify frontend
        await _notificationHubService.SendRefreshNotificationAsync(
            [RefreshDataType.PlexLibrary, RefreshDataType.PlexLibrarySyncStatus],
            ct
        );

        _log.Here().Information("PlexLibrary {PlexLibraryId} enabled and sync queued", plexLibrary.Id);
    }

    private async Task DisableLibraryAsync(PlexLibrary plexLibrary, CancellationToken ct)
    {
        _log.Here().Information("Disabling PlexLibrary {PlexLibraryId}", plexLibrary.Id);

        plexLibrary.IsEnabled = false;
        await _dbContext.SaveChangesNewAsync(ct);

        // Cancel queued/processing sync job for this library
        await _commandExecutor.Send(new CancelLibrarySyncJobCommand(plexLibrary.Id), ct);

        // Purge synced media and derived state based on library type
        await PurgeLibraryMediaAsync(plexLibrary, ct);

        // Reset library sync metadata and counts
        plexLibrary.SyncedAt = null;
        plexLibrary.Outdated = false;
        await _dbContext.SaveChangesNewAsync(ct);

        // Remove all media query cache references to this library and rebuild affected snapshots
        _mediaQueryCache.InvalidateLibrary(plexLibrary.Id, "PlexLibrary disabled");

        // Notify frontend
        await _notificationHubService.SendRefreshNotificationAsync(
            [RefreshDataType.PlexLibrary, RefreshDataType.PlexLibrarySyncStatus],
            ct
        );

        _log.Here().Information("PlexLibrary {PlexLibraryId} disabled and media purged", plexLibrary.Id);
    }

    private async Task PurgeLibraryMediaAsync(PlexLibrary plexLibrary, CancellationToken ct)
    {
        switch (plexLibrary.Type)
        {
            case PlexMediaType.Movie:
            {
                // Delete movies for this library (cascades to movie media data via EF configuration)
                var deleted = await _dbContext.PlexMovies
                    .Where(x => x.PlexLibraryId == plexLibrary.Id)
                    .ExecuteDeleteAsync(ct);

                _log.Here().Information("Purged {Count} PlexMovies from library {PlexLibraryId}", deleted, plexLibrary.Id);
                break;
            }
            case PlexMediaType.TvShow:
            {
                // Delete TV shows
                var deleted = await _dbContext.PlexTvShows
                    .Where(x => x.PlexLibraryId == plexLibrary.Id)
                    .ExecuteDeleteAsync(ct);

                _log.Here().Information("Purged {Count} PlexTvShows from library {PlexLibraryId}", deleted, plexLibrary.Id);
                break;
            }
            default:
                _log.Here().Error("{Type} is not supported", plexLibrary.Type);
                break;
        }
    }
}
