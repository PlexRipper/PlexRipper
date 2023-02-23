using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.FileManager;

public class GetAllFileTasksQueryValidator : AbstractValidator<GetAllFileTasksQuery> { }

public class GetAllFileTasksQueryHandler : BaseHandler, IRequestHandler<GetAllFileTasksQuery, Result<List<DownloadFileTask>>>
{
    public GetAllFileTasksQueryHandler(ILog log, PlexRipperDbContext dbContext) : base(log, dbContext) { }

    public async Task<Result<List<DownloadFileTask>>> Handle(GetAllFileTasksQuery request, CancellationToken cancellationToken)
    {
        var fileTasks = await _dbContext.FileTasks
            .Include(x => x.DownloadTask)
            .ThenInclude(x => x.DownloadWorkerTasks)
            .Include(x => x.DownloadTask)
            .ThenInclude(x => x.DownloadFolder)
            .Include(x => x.DownloadTask)
            .ThenInclude(x => x.DestinationFolder)
            .ToListAsync(cancellationToken);

        return Result.Ok(fileTasks);
    }
}