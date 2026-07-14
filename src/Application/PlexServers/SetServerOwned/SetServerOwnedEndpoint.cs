namespace Reaparr.Application;

public record SetServerOwnedRequest
{
    [RouteParam, BindFrom("PlexServerId")]
    public int PlexServerId { get; init; }

    public bool IsOwned { get; init; }
}

public class SetServerOwnedRequestValidator : Validator<SetServerOwnedRequest>
{
    public SetServerOwnedRequestValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class SetServerOwnedEndpoint : Endpoint<SetServerOwnedRequest, ResultDTO<PlexServerDTO>>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IMediaQueryCache _mediaQueryCache;
    private readonly ICommandExecutor _commandExecutor;

    public SetServerOwnedEndpoint(
        ILogger log,
        IReaparrDbContext dbContext,
        IMediaQueryCache mediaQueryCache,
        ICommandExecutor commandExecutor
    )
    {
        _log = log.ForContext<SetServerOwnedEndpoint>();
        _dbContext = dbContext;
        _mediaQueryCache = mediaQueryCache;
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Put(ApiRoutes.PlexServerController + "/{PlexServerId}/set-server-owned");

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexServerDTO>))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(SetServerOwnedRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);

        var plexServerExists = await _dbContext.PlexServers.IgnoreIsEnabledFilter()
            .AnyAsync(x => x.Id == req.PlexServerId, ct);
        if (!plexServerExists)
        {
            await Send.FluentResult(ResultExtensions.EntityNotFound(nameof(PlexServer), req.PlexServerId), ct);
            return;
        }

        var updateCount = await _dbContext.PlexServers
            .IgnoreIsEnabledFilter()
            .Where(x => x.Id == req.PlexServerId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.OwnedOverride, req.IsOwned), ct);

        if (updateCount == 0)
        {
            await Send.FluentResult(ResultExtensions.EntityNotFound(nameof(PlexServer), req.PlexServerId), ct);
            return;
        }

        var plexServer = await _dbContext.PlexServers
            .IgnoreIsEnabledFilter()
            .Include(x => x.PlexAccountServers)
            .GetAsync(req.PlexServerId, ct);

        if (plexServer is null)
        {
            await Send.FluentResult(ResultExtensions.EntityNotFound(nameof(PlexServer), req.PlexServerId), ct);
            return;
        }

        var libraryIds = await _dbContext.PlexLibraries
            .IgnoreQueryFilters()
            .Where(x => x.PlexServerId == req.PlexServerId)
            .Select(x => x.Id)
            .ToListAsync(ct);
        _mediaQueryCache.InvalidateLibraries(libraryIds, "Plex server ownership scope changed");
        foreach (var libraryId in libraryIds)
            await _commandExecutor.Send(new QueueLibraryComparisonJobsForLibraryCommand(libraryId), ct);

        await Send.FluentResult(Result.Ok(plexServer), x => x.ToDTO(), ct);
    }
}