namespace Reaparr.Application;

public record GetDownloadTaskLogsByDownloadTaskIdRequest
{
    [RouteParam, BindFrom("DownloadTaskGuid")]
    public required Guid DownloadTaskId { get; init; }

    [QueryParam, BindFrom("type")]
    public required DownloadTaskType Type { get; init; }

    [QueryParam, BindFrom("plexServerId")]
    public required int PlexServerId { get; init; }

    [QueryParam, BindFrom("plexLibraryId")]
    public required int PlexLibraryId { get; init; }

    /// <summary>Only return logs with Id greater than this value. Used for incremental polling.</summary>
    [QueryParam, BindFrom("sinceId")]
    public int? SinceId { get; init; }

    /// <summary>Maximum number of logs to return. When null, returns all matching logs.</summary>
    [QueryParam, BindFrom("take")]
    public int? Take { get; init; }
}

public class GetDownloadTaskLogsByDownloadTaskIdRequestValidator : Validator<GetDownloadTaskLogsByDownloadTaskIdRequest>
{
    public GetDownloadTaskLogsByDownloadTaskIdRequestValidator()
    {
        RuleFor(x => x.DownloadTaskId).NotEmpty();
        RuleFor(x => x.Type).NotEqual(DownloadTaskType.None);
        RuleFor(x => x.PlexServerId).GreaterThan(0);
        RuleFor(x => x.PlexLibraryId).GreaterThan(0);
    }
}

public class GetDownloadTaskLogsByDownloadTaskIdEndpoint
    : BaseEndpoint<GetDownloadTaskLogsByDownloadTaskIdRequest, List<DownloadTaskLogDTO>>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.DownloadController + "/logs/{DownloadTaskGuid}/";

    public GetDownloadTaskLogsByDownloadTaskIdEndpoint(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<GetDownloadTaskLogsByDownloadTaskIdEndpoint>();
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<DownloadTaskLogDTO>>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(GetDownloadTaskLogsByDownloadTaskIdRequest req, CancellationToken ct)
    {
        _log.Here().VerboseApiCall(HttpContext, req);

        var key = new DownloadTaskKey
        {
            Type = req.Type,
            Id = req.DownloadTaskId,
            PlexServerId = req.PlexServerId,
            PlexLibraryId = req.PlexLibraryId,
        };

        var logsResult = await _dbContext.GetDownloadTaskLogsAsync(key, req.SinceId, req.Take, ct);

        logsResult.LogIfFailed();

        await Send.FluentResult(logsResult, x => x.Select(log => log.ToDTO()).ToList(), ct);
    }
}
