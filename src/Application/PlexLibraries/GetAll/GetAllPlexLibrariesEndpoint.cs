using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

/// <summary>
/// Retrieves all the <see cref="PlexLibrary">PlexLibraries</see> from the database.
/// </summary>
public class GetAllPlexLibrariesEndpoint : BaseEndpointWithoutRequest<List<PlexLibraryDTO>>
{
    private readonly IReaparrDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexLibraryController + "/";

    public GetAllPlexLibrariesEndpoint(IReaparrDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<PlexLibraryDTO>>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var plexLibraries = await _dbContext.PlexLibraries.ToListAsync(ct);

        await SendFluentResult(Result.Ok(plexLibraries), x => x.ToDTO(), ct);
    }
}
