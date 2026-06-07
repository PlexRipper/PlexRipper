namespace Reaparr.Application;

/// <summary>
/// Retrieves all the <see cref="PlexServer">PlexServers</see>, without PlexLibraries but with all its connections.
/// </summary>
public class GetAllPlexServersEndpoint : EndpointWithoutRequest<List<PlexServerDTO>>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public GetAllPlexServersEndpoint(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<GetAllPlexServersEndpoint>();
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(ApiRoutes.PlexServerController + "/");

        Summary(summary =>
        {
            summary.Summary = "Get All the PlexServers, without PlexLibraries but with all its connections.";
            summary.Description =
                " Retrieves all the PlexServers, without PlexLibraries but with all its connections currently in the database.";
        });
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<PlexServerDTO>>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext);
        var plexServers = await _dbContext
            .PlexServers.IgnoreIsEnabledFilter()
            .Include(x => x.PlexAccountServers)
            .ToListAsync(ct);

        await Send.FluentResult(Result.Ok(plexServers), x => x.ToDTO(), ct);
    }
}
