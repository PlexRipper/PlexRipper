using Application.Contracts;
using Data.Contracts;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public class GetAllFolderPathsEndpoint : BaseEndpointWithoutRequest<List<FolderPathDTO>>
{
    private readonly IPlexRipperDbContext _dbContext;
    public override string EndpointPath => ApiRoutes.FolderPathController + "/";

    public GetAllFolderPathsEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x => x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<FolderPathDTO>>)));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var folderPaths = await _dbContext.FolderPaths.ToListAsync(ct);

        await SendFluentResult(Result.Ok(folderPaths), list => list.ToDTO(), ct);
    }
}
