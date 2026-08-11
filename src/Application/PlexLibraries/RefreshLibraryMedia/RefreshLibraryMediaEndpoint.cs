namespace Reaparr.Application;

public record RefreshLibraryMediaEndpointRequest
{
    [RouteParam]
    public int PlexLibraryId { get; init; }

    public bool ForceLibrarySync { get; init; }

    public bool ForceMediaRefresh { get; init; }
}

public class RefreshLibraryMediaEndpointRequestValidator : Validator<RefreshLibraryMediaEndpointRequest>
{
    public RefreshLibraryMediaEndpointRequestValidator()
    {
        RuleFor(x => x.PlexLibraryId).GreaterThan(0);
    }
}

public class RefreshLibraryMediaEndpoint : Endpoint<RefreshLibraryMediaEndpointRequest, ResultDTO<PlexLibraryDTO>>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    public RefreshLibraryMediaEndpoint(ILogger log, IReaparrDbContext dbContext, ICommandExecutor commandExecutor)
    {
        _log = log.ForContext<RefreshLibraryMediaEndpoint>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Post(ApiRoutes.PlexLibraryController + "/refresh/{PlexLibraryId}");

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexLibraryDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(RefreshLibraryMediaEndpointRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);

        var result = await _commandExecutor.Send(
            new QueueLibrarySyncJobCommand(
                [req.PlexLibraryId],
                req.ForceLibrarySync,
                req.ForceMediaRefresh
            ),
            ct
        );
        if (result.IsFailed)
        {
            await Send.FluentResult(result, ct);
            return;
        }

        var plexLibrary = await _dbContext.PlexLibraries.GetAsync(req.PlexLibraryId, cancellationToken: ct);
        if (plexLibrary is null)
        {
            await Send.FluentResult(ResultExtensions.EntityNotFound(nameof(PlexLibrary), req.PlexLibraryId), ct: ct);
            return;
        }

        await Send.FluentResult(Result.Ok(plexLibrary), x => x.ToDTO(), ct);
    }
}