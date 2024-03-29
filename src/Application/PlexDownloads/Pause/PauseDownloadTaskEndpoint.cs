using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Logging.Interface;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

/// <summary>
/// Pause a currently downloading <see cref="DownloadTaskGeneric"/>.
/// </summary>
/// <param name="DownloadTaskGuid">The id of the <see cref="DownloadTaskGeneric"/> to pause.</param>
/// <returns>Is successful.</returns>
public record PauseDownloadTaskEndpointRequest(Guid DownloadTaskGuid);

public class PauseDownloadTaskEndpointRequestValidator : Validator<PauseDownloadTaskEndpointRequest>
{
    public PauseDownloadTaskEndpointRequestValidator()
    {
        RuleFor(x => x.DownloadTaskGuid).NotEmpty();
    }
}

public class PauseDownloadTaskEndpoint : BaseEndpoint<PauseDownloadTaskEndpointRequest>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly IDownloadTaskScheduler _downloadTaskScheduler;

    public override string EndpointPath => ApiRoutes.DownloadController + "/pause/{DownloadTaskGuid}";

    public PauseDownloadTaskEndpoint(ILog log, IPlexRipperDbContext dbContext, IMediator mediator, IDownloadTaskScheduler downloadTaskScheduler)
    {
        _log = log;
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
            .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(PauseDownloadTaskEndpointRequest req, CancellationToken ct)
    {
        _log.Information("Pausing DownloadTask with id {DownloadTaskGuid} from downloading", req.DownloadTaskGuid);

        var downloadTaskKey = await _dbContext.GetDownloadTaskKeyAsync(req.DownloadTaskGuid, ct);
        if (downloadTaskKey is null)
        {
            await SendFluentResult(ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), req.DownloadTaskGuid), ct);
            return;
        }

        var stopResult = await _downloadTaskScheduler.StopDownloadTaskJob(downloadTaskKey);
        if (stopResult.IsFailed)
        {
            await SendFluentResult(stopResult, ct);
            return;
        }

        await _downloadTaskScheduler.AwaitDownloadTaskJob(req.DownloadTaskGuid, ct);

        _log.Debug("DownloadTask {DownloadTaskId} has been Paused, meaning no downloaded files have been deleted", req.DownloadTaskGuid);

        // Update the download task status
        await _dbContext.SetDownloadStatus(req.DownloadTaskGuid, DownloadStatus.Paused);

        await _mediator.Send(new DownloadTaskUpdatedNotification(downloadTaskKey), ct);

        await SendFluentResult(Result.Ok(), ct);
    }
}