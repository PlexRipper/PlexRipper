using Application.Contracts;
using Data.Contracts;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

/// <summary>
/// Retrieves all the <see cref="PlexLibrary">PlexLibraries</see> from the database.
/// </summary>
public class GetAllPlexLibrariesEndpoint : BaseEndpointWithoutRequest<List<PlexLibraryDTO>>
{
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexLibraryController + "/";

    public GetAllPlexLibrariesEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<PlexLibraryDTO>>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var plexLibraries = await _dbContext.PlexLibraries.ToListAsync(ct);

        await SendFluentResult(Result.Ok(plexLibraries), x => x.ToDTO(), ct);
    }
}
