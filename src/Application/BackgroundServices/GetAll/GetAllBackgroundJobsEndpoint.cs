using System.Diagnostics.CodeAnalysis;
using FastEndpoints;
using Reaparr.Application.Contracts;

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

public class GetAllBackgroundJobsEndpoint : BaseEndpoint<GetAllBackgroundJobsEndpointRequest, List<JobStatusUpdateDTO>>
{
    private readonly ISchedulerService _schedulerService;

    public override string EndpointPath => ApiRoutes.BackgroundJobsController;

    public GetAllBackgroundJobsEndpoint(ISchedulerService schedulerService)
    {
        _schedulerService = schedulerService;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<JobStatusUpdateDTO>>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(GetAllBackgroundJobsEndpointRequest req, CancellationToken ct)
    {
        if (req.UseMockData)
        {
            await SendFluentResult(Result.Ok(MockData()), ct);
        }
        else
        {
            var result = await _schedulerService.GetRunningJobUpdates();

            await SendFluentResult(Result.Ok(result), x => x.ToDTO(), ct);
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

        var syncServerMediaJobUpdate = new JobStatusUpdate<SyncServerMediaJobUpdateDTO>(
            JobTypes.SyncServerMediaJob,
            JobStatus.Started,
            new SyncServerMediaJobUpdateDTO { PlexServerId = 1, ForceSync = true },
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
            syncServerMediaJobUpdate.ToDTO(),
            moveDownloadJobUpdate.ToDTO(),
            checkAllConnectionsStatusJobUpdate.ToDTO(),
        ];
    }
}
