using System.Text.Json;
using Application.Contracts;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public class GetAllBackgroundJobsEndpoint : BaseEndpointWithoutRequest<List<JobStatusUpdateDTO>>
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
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _schedulerService.GetRunningJobUpdates();

        await SendFluentResult(Result.Ok(MockData()), x => x.ToDTO(), ct);
    }

    private List<JobStatusUpdate<string>> MockData()
    {
        var downloadJobUpdatePayload = new DownloadTaskKey
        {
            Type = DownloadTaskType.TvShow,
            Id = Guid.NewGuid(),
            PlexServerId = 1,
            PlexLibraryId = 1,
        };
        var downloadJobUpdate = new JobStatusUpdate<string>(
            JobTypes.DownloadJob,
            JobStatus.Started,
            ToJsonString(downloadJobUpdatePayload),
            Guid.NewGuid().ToString()
        );

        List<int> inspectPlexServerJobUpdatePayload = [1, 2, 3, 4, 5];
        var inspectPlexServerJobUpdate = new JobStatusUpdate<string>(
            JobTypes.InspectPlexServerJob,
            JobStatus.Started,
            ToJsonString(inspectPlexServerJobUpdatePayload),
            Guid.NewGuid().ToString()
        );

        var syncServerMediaJobUpdatePayload = new SyncServerMediaJobUpdateDTO { PlexServerId = 1, ForceSync = true };
        var syncServerMediaJobUpdate = new JobStatusUpdate<string>(
            JobTypes.SyncServerMediaJob,
            JobStatus.Started,
            ToJsonString(syncServerMediaJobUpdatePayload),
            Guid.NewGuid().ToString()
        );

        var fileMergeJobUpdatePayload = new DownloadTaskKey
        {
            Type = DownloadTaskType.Movie,
            Id = Guid.NewGuid(),
            PlexServerId = 2,
            PlexLibraryId = 3,
        };
        var fileMergeJobUpdate = new JobStatusUpdate<string>(
            JobTypes.FileMergeJob,
            JobStatus.Completed,
            ToJsonString(fileMergeJobUpdatePayload),
            Guid.NewGuid().ToString()
        );

        var checkAllConnectionsStatusJobUpdate = new JobStatusUpdate<string>(
            JobTypes.CheckAllConnectionsStatusByPlexServerJob,
            JobStatus.Started,
            string.Empty,
            Guid.NewGuid().ToString()
        );

        return
        [
            downloadJobUpdate,
            inspectPlexServerJobUpdate,
            syncServerMediaJobUpdate,
            fileMergeJobUpdate,
            checkAllConnectionsStatusJobUpdate,
        ];
    }

    private string ToJsonString<T>(T value) =>
        value is null ? string.Empty : JsonSerializer.Serialize(value, DefaultJsonSerializerOptions.ConfigStandard);
}
