using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

/// <summary>
/// Retrieves all <see cref="PlexAccount"/>s with the included <see cref="PlexServer"/>s and <see cref="PlexLibrary"/>s.
/// </summary>
/// <returns>A list of all <see cref="PlexAccount"/>s.</returns>
public record GetAllPlexAccountsEndpointRequest
{
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
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexAccountController;

    public GetAllPlexAccountsEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<PlexAccountDTO>>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(GetAllPlexAccountsEndpointRequest req, CancellationToken ct)
    {
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
