using Data.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record GetDownloadTaskByIdQuery(int DownloadTaskId) : IRequest<Result<DownloadTask>>;

public class GetDownloadTaskByIdQueryValidator : AbstractValidator<GetDownloadTaskByIdQuery>
{
    public GetDownloadTaskByIdQueryValidator()
    {
        RuleFor(x => x.DownloadTaskId).GreaterThan(0);
    }
}

public class GetDownloadTaskByIdQueryHandler : IRequestHandler<GetDownloadTaskByIdQuery, Result<DownloadTask>>
{
    private readonly IPlexRipperDbContext _dbContext;

    public GetDownloadTaskByIdQueryHandler(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<DownloadTask>> Handle(GetDownloadTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var downloadTask = await
            _dbContext.DownloadTasks.AsTracking()
                .Include(x => x.PlexServer)
                .Include(x => x.PlexLibrary)
                .Include(x => x.DestinationFolder)
                .Include(x => x.DownloadFolder)
                .Include(x => x.DownloadWorkerTasks)
                .IncludeDownloadTasks()
                .GetAsync(request.DownloadTaskId, cancellationToken);

        if (downloadTask is null)
            return ResultExtensions.EntityNotFound(nameof(DownloadTask), request.DownloadTaskId);

        return Result.Ok(downloadTask);
    }
}