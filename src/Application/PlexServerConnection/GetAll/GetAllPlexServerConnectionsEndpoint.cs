using Application.Contracts;
using Data.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public class GetAllPlexServerConnectionsEndpoint : BaseEndpointWithoutRequest<List<PlexServerConnectionDTO>>
{
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexServerConnectionController + "/";

    public GetAllPlexServerConnectionsEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<PlexServerConnectionDTO>>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var plexServerConnections = await _dbContext
            .PlexServerConnections.Include(x => x.PlexServerStatus.OrderByDescending(y => y.LastChecked).Take(5))
            .ToListAsync(ct);

        await SendFluentResult(Result.Ok(plexServerConnections), x => x.ToDTO(), ct);
    }
}
