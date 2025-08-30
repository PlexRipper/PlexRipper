using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

public record DeleteDownloadTaskEndpointRequest
{
    [FromBody]
    public required List<Guid> DownloadTaskIds { get; init; }
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
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IDownloadTaskScheduler _downloadTaskScheduler;

    public override string EndpointPath => ApiRoutes.DownloadController + "/delete";

    public DeleteDownloadTaskEndpoint(
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor,
        IDownloadTaskScheduler downloadTaskScheduler
    )
    {
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
        _downloadTaskScheduler = downloadTaskScheduler;
    }

    public override void Configure()
    {
        Delete(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(DeleteDownloadTaskEndpointRequest req, CancellationToken ct)
    {
        foreach (var downloadTaskId in req.DownloadTaskIds)
        {
            var downloadTaskKey = await _dbContext.GetDownloadTaskKeyAsync(downloadTaskId, ct);
            if (downloadTaskKey is not null && await _downloadTaskScheduler.IsDownloading(downloadTaskKey, ct))
            {
                var stopResult = await _commandExecutor.Send(new StopDownloadTaskCommand(downloadTaskKey.Id), ct);
                if (stopResult.IsFailed)
                {
                    await SendFluentResult(stopResult, ct);
                    return;
                }
            }
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
