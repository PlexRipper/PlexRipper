using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

public class GetAllDownloadTasksEndpoint : BaseEndpointWithoutRequest<List<ServerDownloadProgressDTO>>
{
    private readonly Serilog.ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.DownloadController;

    public GetAllDownloadTasksEndpoint(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<GetAllDownloadTasksEndpoint>();
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<ServerDownloadProgressDTO>>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext);
        var downloadList = await _dbContext.GetAllDownloadTasksByServerAsync(cancellationToken: ct);
        await SendFluentResult(Result.Ok(downloadList), x => x.ToServerDownloadProgressDTOList(), ct);
    }
}
