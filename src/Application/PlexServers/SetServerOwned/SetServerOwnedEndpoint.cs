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

public class SetServerOwnedEndpoint : BaseEndpoint<SetServerOwnedRequest, PlexServerDTO>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexServerController + "/{PlexServerId}/set-server-owned";

    public SetServerOwnedEndpoint(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<SetServerOwnedEndpoint>();
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Put(EndpointPath);

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
            await SendFluentResult(ResultExtensions.EntityNotFound(nameof(PlexServer), req.PlexServerId), ct);
            return;
        }

        var updateCount = await _dbContext.PlexServers
            .IgnoreIsEnabledFilter()
            .Where(x => x.Id == req.PlexServerId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.OwnedOverride, req.IsOwned), ct);

        if (updateCount == 0)
        {
            await SendFluentResult(ResultExtensions.EntityNotFound(nameof(PlexServer), req.PlexServerId), ct);
            return;
        }

        var plexServer = await _dbContext.PlexServers
            .IgnoreIsEnabledFilter()
            .Include(x => x.PlexAccountServers)
            .GetAsync(req.PlexServerId, ct);

        if (plexServer is null)
        {
            await SendFluentResult(ResultExtensions.EntityNotFound(nameof(PlexServer), req.PlexServerId), ct);
            return;
        }

        await SendFluentResult(Result.Ok(plexServer), x => x.ToDTO(), ct);
    }
}