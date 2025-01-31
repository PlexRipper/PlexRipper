using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public record GetDownloadTaskByGuidRequest
{
    /// <summary>
    /// NOTE: This constructor is needed to make the query param optional in the front-end typescript-api generation.
    /// </summary>
    public GetDownloadTaskByGuidRequest(DownloadTaskType type = DownloadTaskType.None)
    {
        Type = type;
    }

    public required Guid DownloadTaskGuid { get; init; }

    [QueryParam, BindFrom("type")]
    public DownloadTaskType Type { get; init; }
}

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

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<DownloadTaskDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(GetDownloadTaskByGuidRequest req, CancellationToken ct)
    {
        var downloadTask = await _dbContext.GetDownloadTaskAsync(req.DownloadTaskGuid, req.Type, ct);

        if (downloadTask is null)
        {
            await SendFluentResult(
                ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), req.DownloadTaskGuid).LogError(),
                ct
            );
            return;
        }

        // Add DownloadUrl to DownloadTaskDTO
        if (downloadTask.IsDownloadable)
        {
            var downloadUrl = await _dbContext.GetDownloadUrl(
                downloadTask.PlexServerId,
                downloadTask.FileLocationUrl,
                ct
            );
            if (downloadUrl.IsFailed)
                downloadUrl.LogError();

            await SendFluentResult(Result.Ok(downloadTask), x => x.ToDTO(downloadUrl.ValueOrDefault), ct);
            return;
        }

        await SendFluentResult(Result.Ok(downloadTask), x => x.ToDTO(), ct);
    }
}
