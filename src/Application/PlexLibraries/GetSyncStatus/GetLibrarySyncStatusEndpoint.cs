namespace Reaparr.Application;

/// <summary>
/// Retrieves all the <see cref="LibrarySyncJobQueue">LibrarySyncJobQueues</see> from the database.
/// </summary>
public class GetLibrarySyncStatusEndpoint : BaseEndpointWithoutRequest<List<LibrarySyncJobQueueDTO>>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public GetLibrarySyncStatusEndpoint(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<GetLibrarySyncStatusEndpoint>();
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(ApiRoutes.PlexLibraryController + "/sync-status");

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<LibrarySyncJobQueueDTO>>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext);
        var syncJobs = await _dbContext.LibrarySyncJobQueues.ToListAsync(ct);

        await Send.FluentResult(Result.Ok(syncJobs), x => x.Select(y => y.ToDTO()).ToList(), ct);
    }
}
