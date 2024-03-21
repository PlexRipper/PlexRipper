using Application.Contracts;
using Data.Contracts;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public class GetAllDownloadTasksEndpoint : BaseEndpointWithoutRequest<List<ServerDownloadProgressDTO>>
{
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.DownloadController;

    public GetAllDownloadTasksEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x => x
            .Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<ServerDownloadProgressDTO>>))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var downloadList = await _dbContext.GetAllDownloadTasksAsync(cancellationToken: ct);
        await SendFluentResult(Result.Ok(downloadList), x => x.ToServerDownloadProgressDTOList(), ct);
    }
}