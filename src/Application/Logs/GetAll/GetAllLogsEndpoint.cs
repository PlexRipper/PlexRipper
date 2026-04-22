namespace Reaparr.Application;

public class GetAllLogsEndpoint : BaseEndpointWithoutRequest<List<LiveLogEventDTO>>
{
    private readonly ILogger _log;
    private readonly ILogBufferService _logBufferService;

    public override string EndpointPath => ApiRoutes.DebugController + "/logs/";

    public GetAllLogsEndpoint(ILogger log, ILogBufferService logBufferService)
    {
        _log = log.ForContext<GetAllLogsEndpoint>();
        _logBufferService = logBufferService;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<LiveLogEventDTO>>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        _log.Here().VerboseApiCall(HttpContext);
        await SendFluentResult(Result.Ok(_logBufferService.GetAll().ToList()), x => x, ct);
    }
}
