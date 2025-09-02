using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

/// <summary>
/// Retrieves all the <see cref="PlexServer">PlexServers</see>, without PlexLibraries but with all its connections.
/// </summary>
public class GetAllPlexServersEndpoint : BaseEndpointWithoutRequest<List<PlexServerDTO>>
{
    private readonly Serilog.ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexServerController + "/";

    public GetAllPlexServersEndpoint(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<GetAllPlexServersEndpoint>();
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);

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
        var plexServers = await _dbContext.PlexServers.Include(x => x.PlexAccountServers).ToListAsync(ct);

        await SendFluentResult(Result.Ok(plexServers), x => x.ToDTO(), ct);
    }
}
