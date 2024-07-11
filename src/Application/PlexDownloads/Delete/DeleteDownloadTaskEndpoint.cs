using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public class DeleteDownloadTaskEndpointRequest
{
    [FromBody]
    public List<Guid> DownloadTaskIds { get; init; }
}

public class DeleteDownloadTaskEndpointRequestValidator : Validator<DeleteDownloadTaskEndpointRequest>
{
    public DeleteDownloadTaskEndpointRequestValidator()
    {
        RuleFor(x => x.DownloadTaskIds).NotEmpty();
    }
}

public class DeleteDownloadTaskEndpoint : BaseEndpoint<DeleteDownloadTaskEndpointRequest>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly IDownloadTaskScheduler _downloadTaskScheduler;

    public override string EndpointPath => ApiRoutes.DownloadController + "/delete";

    public DeleteDownloadTaskEndpoint(
        IPlexRipperDbContext dbContext,
        IMediator mediator,
        IDownloadTaskScheduler downloadTaskScheduler
    )
    {
        _dbContext = dbContext;
        _mediator = mediator;
        _downloadTaskScheduler = downloadTaskScheduler;
    }

    public override void Configure()
    {
        Delete(EndpointPath);
        AllowAnonymous();
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO))
                .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(DeleteDownloadTaskEndpointRequest req, CancellationToken ct)
    {
        foreach (var downloadTaskId in req.DownloadTaskIds)
        {
            var downloadTaskKey = await _dbContext.GetDownloadTaskKeyAsync(downloadTaskId, ct);
            if (downloadTaskKey is not null && await _downloadTaskScheduler.IsDownloading(downloadTaskKey))
                await _mediator.Send(new StopDownloadTaskCommand(downloadTaskKey.Id), ct);
        }

        // Delete Download tasks
        await _dbContext.DownloadTaskMovie.Where(x => req.DownloadTaskIds.Contains(x.Id)).ExecuteDeleteAsync(ct);
        await _dbContext.DownloadTaskMovieFile.Where(x => req.DownloadTaskIds.Contains(x.Id)).ExecuteDeleteAsync(ct);
        await _dbContext.DownloadTaskTvShow.Where(x => req.DownloadTaskIds.Contains(x.Id)).ExecuteDeleteAsync(ct);
        await _dbContext.DownloadTaskTvShowSeason.Where(x => req.DownloadTaskIds.Contains(x.Id)).ExecuteDeleteAsync(ct);
        await _dbContext
            .DownloadTaskTvShowEpisode.Where(x => req.DownloadTaskIds.Contains(x.Id))
            .ExecuteDeleteAsync(ct);
        await _dbContext
            .DownloadTaskTvShowEpisodeFile.Where(x => req.DownloadTaskIds.Contains(x.Id))
            .ExecuteDeleteAsync(ct);

        await SendFluentResult(Result.Ok(), ct);
    }
}
