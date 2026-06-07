namespace Reaparr.Application;

public record DeletePlexServerEndpointRequest
{
    [RouteParam, BindFrom("PlexServerId")]
    public int PlexServerId { get; init; }
}

public class DeletePlexServerEndpointRequestValidator : Validator<DeletePlexServerEndpointRequest>
{
    public DeletePlexServerEndpointRequestValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class DeletePlexServerEndpoint : Endpoint<DeletePlexServerEndpointRequest, BaseResultDTO>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public DeletePlexServerEndpoint(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<DeletePlexServerEndpoint>();
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Delete(ApiRoutes.PlexServerController + "/{PlexServerId}");

        Description(x =>
            x.Accepts<DeletePlexServerEndpointRequest>()
                .Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(DeletePlexServerEndpointRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);

        var plexServer = await _dbContext.PlexServers
            .IgnoreIsEnabledFilter()
            .GetAsync(req.PlexServerId, ct);

        if (plexServer is null)
        {
            await Send.FluentResult(ResultExtensions.EntityNotFound(nameof(PlexServer), req.PlexServerId), ct);
            return;
        }

        _dbContext.PlexServers.Remove(plexServer);
        await _dbContext.SaveChangesAsync(ct);

        await Send.FluentResult(Result.Ok(), ct);
    }
}
