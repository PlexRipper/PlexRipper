using Application.Contracts;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public record GetAllBackgroundJobsEndpointRequest
{
    [QueryParam, BindFrom("UseMockData")]
    public bool UseMockData { get; init; } = false;
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

        var fileMergeJobUpdate = new JobStatusUpdate<FileMergeJobUpdateDTO>(
            JobTypes.FileMergeJob,
            JobStatus.Completed,
            new FileMergeJobUpdateDTO
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
            fileMergeJobUpdate.ToDTO(),
            checkAllConnectionsStatusJobUpdate.ToDTO(),
        ];
    }
}
