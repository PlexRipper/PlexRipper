namespace Reaparr.Application;

public record GetPlexAccountByIdEndpointRequest(int PlexAccountId);

public class GetPlexAccountByIdEndpointRequestValidator : Validator<GetPlexAccountByIdEndpointRequest>
{
    public GetPlexAccountByIdEndpointRequestValidator()
    {
        RuleFor(x => x.PlexAccountId).GreaterThan(0);
    }
}

public class GetPlexAccountByIdEndpoint : BaseEndpoint<GetPlexAccountByIdEndpointRequest, PlexAccountDTO>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public GetPlexAccountByIdEndpoint(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<GetPlexAccountByIdEndpoint>();
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(ApiRoutes.PlexAccountController + "/{PlexAccountId}");

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexAccountDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(GetPlexAccountByIdEndpointRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);
        var plexAccount = await _dbContext
            .PlexAccounts.Include(x => x.PlexAccountServers)
            .Include(x => x.PlexAccountLibraries)
            .GetAsync(req.PlexAccountId, ct);
        if (plexAccount is null)
        {
            await Send.FluentResult(
                ResultExtensions.EntityNotFound(nameof(PlexAccount), req.PlexAccountId).LogWarning(),
                ct
            );
            return;
        }

        _log.Here()
            .Debug("Found an {NameOfPlexAccount} with the id: {AccountId}", nameof(PlexAccount), req.PlexAccountId);
        await Send.FluentResult(Result.Ok(plexAccount), x => x.ToDTO(), ct);
    }
}
