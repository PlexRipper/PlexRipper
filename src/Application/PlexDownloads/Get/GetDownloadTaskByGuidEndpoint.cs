using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public record GetDownloadTaskByGuidRequest(Guid DownloadTaskGuid, DownloadTaskType Type = DownloadTaskType.None);

public class GetDownloadTaskByGuidRequestValidator : Validator<GetDownloadTaskByGuidRequest>
{
    public GetDownloadTaskByGuidRequestValidator()
    {
        RuleFor(x => x.DownloadTaskGuid).NotEmpty();
        RuleFor(x => x.DownloadTaskGuid).NotEqual(Guid.Empty);
    }
}

public class GetDownloadTaskByGuidEndpoint : BaseEndpoint<GetDownloadTaskByGuidRequest, DownloadTaskDTO>
{
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.DownloadController + "/detail/{DownloadTaskGuid}";

    public GetDownloadTaskByGuidEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x => x
            .Produces(StatusCodes.Status200OK, typeof(ResultDTO<DownloadTaskDTO>))
            .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
            .Produces(StatusCodes.Status404NotFound, typeof(ResultDTO))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(GetDownloadTaskByGuidRequest req, CancellationToken ct)
    {
        var downloadTask = await _dbContext.GetDownloadTaskAsync(req.DownloadTaskGuid, req.Type, ct);

        if (downloadTask is null)
        {
            await SendFluentResult(ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), req.DownloadTaskGuid).LogError(), ct);
            return;
        }

        var downloadTaskDto = downloadTask.ToDTO();
        if (!downloadTask.IsDownloadable)
        {
            await SendFluentResult(Result.Ok(downloadTaskDto), x => x, ct);
            return;
        }

        // Add DownloadUrl to DownloadTaskDTO
        var downloadUrl =
            await _dbContext.GetDownloadUrl(downloadTaskDto.PlexServerId, downloadTaskDto.FileLocationUrl, ct);
        if (downloadUrl.IsFailed)
        {
            await SendFluentResult(downloadUrl.ToResult(), ct);
            return;
        }

        downloadTaskDto.DownloadUrl = downloadUrl.Value;

        await SendFluentResult(Result.Ok(downloadTaskDto), x => x, ct);
    }
}