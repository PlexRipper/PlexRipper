using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.Logging;
using Serilog;

namespace Reaparr.Application;

/// <summary>
/// Retrieves all <see cref="PlexAccount"/>s with the included <see cref="PlexServer"/>s and <see cref="PlexLibrary"/>s.
/// </summary>
/// <returns>A list of all <see cref="PlexAccount"/>s.</returns>
public record GetAllPlexAccountsEndpointRequest
{
    /// <summary>
    /// NOTE: This constructor is needed to make the query param optional in the front-end typescript-api generation.
    /// </summary>
    public GetAllPlexAccountsEndpointRequest(bool enabledOnly = false)
    {
        EnabledOnly = enabledOnly;
    }

    /// <summary>
    /// Should only return enabled <see cref="PlexAccount">PlexAccounts</see>.
    /// </summary>
    [QueryParam, BindFrom("enabledOnly")]
    public bool EnabledOnly { get; init; }
}

public class GetAllPlexAccountsEndpoint : BaseEndpoint<GetAllPlexAccountsEndpointRequest, List<PlexAccountDTO>>
{
    private readonly Serilog.ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexAccountController;

    public GetAllPlexAccountsEndpoint(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<GetAllPlexAccountsEndpoint>();
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<PlexAccountDTO>>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(GetAllPlexAccountsEndpointRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);
        var query = _dbContext
            .PlexAccounts.Include(x => x.PlexAccountServers)
            .Include(x => x.PlexAccountLibraries)
            .AsQueryable();
        if (req.EnabledOnly)
            query = query.Where(x => x.IsEnabled);

        var plexAccounts = await query.ToListAsync(ct);

        await SendFluentResult(Result.Ok(plexAccounts), x => x.ToDTO(), ct);
    }
}
