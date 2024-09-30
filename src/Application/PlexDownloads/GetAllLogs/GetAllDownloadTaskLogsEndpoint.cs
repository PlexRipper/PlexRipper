using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record GetDownloadTaskLogsByDownloadTaskIdRequest(Guid DownloadTaskGuid);

public class GetDownloadTaskLogsByDownloadTaskIdRequestValidator : Validator<GetDownloadTaskLogsByDownloadTaskIdRequest>
{
    public GetDownloadTaskLogsByDownloadTaskIdRequestValidator()
    {
        RuleFor(x => x.DownloadTaskGuid).NotEmpty();
        RuleFor(x => x.DownloadTaskGuid).NotEqual(Guid.Empty);
    }
}

public class GetDownloadTaskLogsByDownloadTaskIdEndpoint
    : BaseEndpoint<GetDownloadTaskLogsByDownloadTaskIdRequest, List<DownloadWorkerLogDTO>>
{
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.DownloadController + "/logs/{DownloadTaskGuid}/";

    public GetDownloadTaskLogsByDownloadTaskIdEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<DownloadWorkerLogDTO>>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(GetDownloadTaskLogsByDownloadTaskIdRequest req, CancellationToken ct)
    {
        var key = await _dbContext.GetDownloadTaskKeyAsync(req.DownloadTaskGuid, ct);

        var childTasks = await _dbContext.GetDownloadableChildTasks(key, ct);

        var downloadWorkerIds = childTasks.Select(x => x.Id).ToList();

        var logs = await _dbContext
            .DownloadWorkerTasksLogs.Where(x => downloadWorkerIds.Contains(x.DownloadTaskId))
            .ToListAsync(ct);

        await SendFluentResult(Result.Ok(logs), x => x.ToDTO(), ct);
    }
}
