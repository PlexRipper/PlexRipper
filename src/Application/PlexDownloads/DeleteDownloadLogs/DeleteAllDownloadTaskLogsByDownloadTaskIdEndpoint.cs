namespace Reaparr.Application;

public record DeleteAllDownloadTaskLogsByDownloadTaskIdRequest
{
    [RouteParam, BindFrom("DownloadTaskGuid")]
    public required Guid DownloadTaskId { get; init; }

    [QueryParam, BindFrom("type")]
    public required DownloadTaskType Type { get; init; }

    [QueryParam, BindFrom("plexServerId")]
    public required int PlexServerId { get; init; }

    [QueryParam, BindFrom("plexLibraryId")]
    public required int PlexLibraryId { get; init; }
}

public class DeleteAllDownloadTaskLogsByDownloadTaskIdRequestValidator
    : Validator<DeleteAllDownloadTaskLogsByDownloadTaskIdRequest>
{
    public DeleteAllDownloadTaskLogsByDownloadTaskIdRequestValidator()
    {
        RuleFor(x => x.DownloadTaskId).NotEmpty();
        RuleFor(x => x.Type).NotEqual(DownloadTaskType.None);
        RuleFor(x => x.PlexServerId).GreaterThan(0);
        RuleFor(x => x.PlexLibraryId).GreaterThan(0);
    }
}

public class DeleteAllDownloadTaskLogsByDownloadTaskIdEndpoint
    : BaseEndpoint<DeleteAllDownloadTaskLogsByDownloadTaskIdRequest, int>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.DownloadController + "/logs/{DownloadTaskGuid}/";

    public DeleteAllDownloadTaskLogsByDownloadTaskIdEndpoint(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<DeleteAllDownloadTaskLogsByDownloadTaskIdEndpoint>();
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Delete(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<int>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(DeleteAllDownloadTaskLogsByDownloadTaskIdRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);

        var key = new DownloadTaskKey
        {
            Type = req.Type,
            Id = req.DownloadTaskId,
            PlexServerId = req.PlexServerId,
            PlexLibraryId = req.PlexLibraryId,
        };
        var logsResult = await _dbContext.DeleteDownloadTaskLogsAsync(key, ct);

        logsResult.LogIfFailed();

        if (logsResult.IsSuccess)
            _log.Here().Debug("Deleted {Count} logs of type {DownloadTaskType}", logsResult.Value, key.Type);

        await SendFluentResult(logsResult, x => x, ct);
    }
}
