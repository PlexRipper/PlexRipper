using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.FileManager;

public class GetFileTaskByIdQueryValidator : AbstractValidator<GetFileTaskByIdQuery>
{
    public GetFileTaskByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class GetFileTaskByIdQueryHandler : BaseHandler, IRequestHandler<GetFileTaskByIdQuery, Result<DownloadFileTask>>
{
    public GetFileTaskByIdQueryHandler(ILog log, PlexRipperDbContext dbContext) : base(log, dbContext) { }

    public async Task<Result<DownloadFileTask>> Handle(GetFileTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var fileTask = await _dbContext.FileTasks
            .Include(x => x.DownloadTask)
            .ThenInclude(x => x.DownloadWorkerTasks)
            .Include(x => x.DownloadTask)
            .ThenInclude(x => x.DownloadFolder)
            .Include(x => x.DownloadTask)
            .ThenInclude(x => x.DestinationFolder)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (fileTask == null)
            return ResultExtensions.EntityNotFound(nameof(DownloadFileTask), request.Id);

        return Result.Ok(fileTask);
    }
}