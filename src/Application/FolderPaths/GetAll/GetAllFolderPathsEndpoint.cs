using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public class GetAllFolderPathsEndpoint : BaseCustomEndpointWithoutRequest
{
    private readonly IPlexRipperDbContext _dbContext;
    public override string EndpointPath => throw new NotImplementedException();

    public GetAllFolderPathsEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Verbs(Http.GET);
        Post(EndpointPath);
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var folderPaths = await _dbContext.FolderPaths.ToListAsync(ct);

        await SendResult(Result.Ok(folderPaths));
    }
}