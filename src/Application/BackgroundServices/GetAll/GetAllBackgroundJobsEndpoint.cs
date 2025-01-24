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

        await SendFluentResult(Result.Ok(result), x => x.ToDTO(), ct);
    }
}
