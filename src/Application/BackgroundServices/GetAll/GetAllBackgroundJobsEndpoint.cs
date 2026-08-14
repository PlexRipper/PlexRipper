namespace Reaparr.Application;

public record GetAllBackgroundJobsEndpointRequest
{
    /// <summary>
    /// NOTE: This constructor is needed to make the query param optional in the front-end typescript-api generation.
    /// </summary>
    [SetsRequiredMembers]
    public GetAllBackgroundJobsEndpointRequest(bool useMockData = false)
    {
        UseMockData = useMockData;
    }

    [QueryParam, BindFrom("UseMockData")]
    public required bool UseMockData { get; init; }
}

public class GetAllBackgroundJobsEndpoint
    : Endpoint<GetAllBackgroundJobsEndpointRequest, ResultDTO<List<JobStatusUpdateDTO>>>
{
    private readonly IBackgroundJobScheduler _backgroundJobScheduler;

    public GetAllBackgroundJobsEndpoint(IBackgroundJobScheduler backgroundJobScheduler)
    {
        _backgroundJobScheduler = backgroundJobScheduler;
    }

    public override void Configure()
    {
        Get(ApiRoutes.BackgroundJobsController);
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<JobStatusUpdateDTO>>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(GetAllBackgroundJobsEndpointRequest req, CancellationToken ct)
    {
        if (req.UseMockData)
        {
            await Send.FluentResult(Result.Ok(MockData()), ct);
        }
        else
        {
            var result = await _backgroundJobScheduler.GetCurrentlyExecutingJobs(ct);

            await Send.FluentResult(Result.Ok(result), x => x.ToDTO(), ct);
        }
    }

    private List<JobStatusUpdateDTO> MockData()
    {
        var downloadJobUpdate = new JobStatusUpdate<DownloadJobUpdateDTO>(
            JobTypes.DownloadJob,
            JobStatus.Started,
            new DownloadJobUpdateDTO
            {
                Id = new DownloadTaskKey
                {
                    Type = DownloadTaskType.TvShow,
                    Id = Guid.NewGuid(),
                    PlexServerId = 1,
                    PlexLibraryId = 1,
                },
            },
            Guid.NewGuid().ToString()
        );

        var inspectPlexServerJobUpdate = new JobStatusUpdate<InspectPlexServerJobUpdateDTO>(
            JobTypes.InspectPlexServerJob,
            JobStatus.Started,
            new InspectPlexServerJobUpdateDTO { PlexServerIds = [1, 2, 3, 4, 5] },
            Guid.NewGuid().ToString()
        );

        var moveDownloadJobUpdate = new JobStatusUpdate<MoveDownloadFileJobUpdateDTO>(
            JobTypes.MoveDownloadFileJob,
            JobStatus.Completed,
            new MoveDownloadFileJobUpdateDTO
            {
                DownloadTaskId = new DownloadTaskKey
                {
                    Type = DownloadTaskType.Movie,
                    Id = Guid.NewGuid(),
                    PlexServerId = 2,
                    PlexLibraryId = 3,
                },
            },
            Guid.NewGuid().ToString()
        );

        var checkAllConnectionsStatusJobUpdate = new JobStatusUpdate<CheckAllConnectionStatusUpdateDTO>(
            JobTypes.CheckAllConnectionsStatusByPlexServerJob,
            JobStatus.Started,
            new CheckAllConnectionStatusUpdateDTO { PlexServersWithConnectionIds = new Dictionary<int, List<int>>() },
            Guid.NewGuid().ToString()
        );

        return
        [
            downloadJobUpdate.ToDTO(),
            inspectPlexServerJobUpdate.ToDTO(),
            moveDownloadJobUpdate.ToDTO(),
            checkAllConnectionsStatusJobUpdate.ToDTO(),
        ];
    }
}
