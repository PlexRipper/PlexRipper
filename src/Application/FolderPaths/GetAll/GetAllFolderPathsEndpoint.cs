using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

public class GetAllFolderPathsEndpoint : BaseEndpointWithoutRequest<List<FolderPathDTO>>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    public override string EndpointPath => ApiRoutes.FolderPathController + "/";

    public GetAllFolderPathsEndpoint(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<GetAllFolderPathsEndpoint>();
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x => x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<FolderPathDTO>>)));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext);
        var folderPaths = await _dbContext.FolderPaths.ToListAsync(ct);

        await SendFluentResult(Result.Ok(folderPaths), list => list.ToDTO(), ct);
    }
}
