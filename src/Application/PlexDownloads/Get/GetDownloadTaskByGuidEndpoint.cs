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

        // Add DownloadUrl to DownloadTaskDTO
        if (!downloadTask.IsDownloadable)
        {
            var downloadUrl =
                await _dbContext.GetDownloadUrl(downloadTask.PlexServerId, downloadTask.FileLocationUrl, ct);
            if (downloadUrl.IsFailed)
                downloadUrl.LogError();

            await SendFluentResult(Result.Ok(downloadTask), x => x.ToDTO(downloadUrl.ValueOrDefault), ct);
            return;
        }

        await SendFluentResult(Result.Ok(downloadTask), x => x.ToDTO(), ct);
    }
}