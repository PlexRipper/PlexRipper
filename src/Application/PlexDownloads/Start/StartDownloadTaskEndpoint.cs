using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public record StartDownloadTaskEndpointRequest(Guid DownloadTaskGuid);

public class StartDownloadTaskEndpointRequestValidator : Validator<StartDownloadTaskEndpointRequest>
{
    public StartDownloadTaskEndpointRequestValidator()
    {
        RuleFor(x => x.DownloadTaskGuid).NotEmpty();
    }
}

public class StartDownloadTaskEndpoint : BaseEndpoint<StartDownloadTaskEndpointRequest>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly IDownloadTaskScheduler _downloadTaskScheduler;

    public override string EndpointPath => ApiRoutes.DownloadController + "/start/{DownloadTaskGuid}";

    public StartDownloadTaskEndpoint(IPlexRipperDbContext dbContext, IMediator mediator, IDownloadTaskScheduler downloadTaskScheduler)
    {
        _dbContext = dbContext;
        _mediator = mediator;
        _downloadTaskScheduler = downloadTaskScheduler;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x => x
            .Produces(StatusCodes.Status200OK, typeof(ResultDTO))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(StartDownloadTaskEndpointRequest req, CancellationToken ct)
    {
        var downloadTask = await _dbContext.GetDownloadTaskAsync(req.DownloadTaskGuid, cancellationToken: ct);
        if (downloadTask is null)
        {
            await SendFluentResult(ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), req.DownloadTaskGuid).LogWarning(), ct);
            return;
        }

        if (downloadTask.IsDownloadable)
        {
            var startResult = await _downloadTaskScheduler.StartDownloadTaskJob(downloadTask.ToKey());
            await SendFluentResult(startResult, ct);

            await _mediator.Publish(new CheckDownloadQueueNotification(downloadTask.PlexServerId), ct);
            return;
        }

        await SendFluentResult(Result.Fail($"Failed to start downloadTask {downloadTask.FullTitle}, it's not directly downloadable."), ct);
    }
}