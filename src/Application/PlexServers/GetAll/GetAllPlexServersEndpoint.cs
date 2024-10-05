using Application.Contracts;
using Data.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NaturalSort.Extension;

namespace PlexRipper.Application;

/// <summary>
/// Retrieves all the <see cref="PlexServer">PlexServers</see>, without PlexLibraries but with all its connections.
/// </summary>
public class GetAllPlexServersEndpoint : BaseEndpointWithoutRequest<List<PlexServerDTO>>
{
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexServerController + "/";

    public GetAllPlexServersEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Summary(summary =>
        {
            summary.Summary = "Get All the PlexServers, without PlexLibraries but with all its connections.";
            summary.Description =
                " Retrieves all the PlexServers, without PlexLibraries but with all its connections currently in the database.";
        });
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<PlexServerDTO>>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var plexServers = await _dbContext.PlexServers.ToListAsync(ct);

        plexServers = plexServers
            .OrderByDescending(x => x.Owned)
            .ThenBy(x => x.Name, StringComparison.OrdinalIgnoreCase.WithNaturalSort())
            .ToList();

        await SendFluentResult(Result.Ok(plexServers), x => x.ToDTO(), ct);
    }
}
