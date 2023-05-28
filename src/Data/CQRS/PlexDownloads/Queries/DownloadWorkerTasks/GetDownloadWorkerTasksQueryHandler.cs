using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data;

public class GetDownloadWorkerTasksByDownloadTaskIdQueryValidator : AbstractValidator<GetAllDownloadWorkerTasksByDownloadTaskIdQuery>
{
    public GetDownloadWorkerTasksByDownloadTaskIdQueryValidator()
    {
        RuleFor(x => x.DownloadTaskId).GreaterThan(0);
    }
}

public class GetDownloadWorkerTasksByDownloadTaskIdQueryHandler : BaseHandler,
    IRequestHandler<GetAllDownloadWorkerTasksByDownloadTaskIdQuery, Result<List<DownloadWorkerTask>>>
{
    public GetDownloadWorkerTasksByDownloadTaskIdQueryHandler(ILog log, PlexRipperDbContext dbContext) : base(log, dbContext) { }

    public async Task<Result<List<DownloadWorkerTask>>> Handle(GetAllDownloadWorkerTasksByDownloadTaskIdQuery request, CancellationToken cancellationToken)
    {
        var downloadWorkerTasks = await _dbContext.DownloadWorkerTasks
            .Where(x => x.DownloadTaskId == request.DownloadTaskId)
            .ToListAsync(cancellationToken);

        return ReturnResult(downloadWorkerTasks, request.DownloadTaskId);
    }
}