using Application.Contracts;
using Data.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public class GetAllFolderPathsEndpoint : BaseCustomEndpointWithoutRequest
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
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<FolderPathDTO>>)));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var folderPaths = await _dbContext.FolderPaths.ToListAsync(ct);

        await SendResult(Result.Ok(folderPaths.ToDTO()), ct);
    }
}