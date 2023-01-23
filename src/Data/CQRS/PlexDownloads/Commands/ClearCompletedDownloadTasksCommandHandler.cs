using Data.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;

namespace PlexRipper.Data;

public class ClearCompletedDownloadTasksCommandValidator : AbstractValidator<ClearCompletedDownloadTasksCommand> { }

public class ClearCompletedDownloadTasksHandler : BaseHandler,
    IRequestHandler<ClearCompletedDownloadTasksCommand, Result>
{
    public ClearCompletedDownloadTasksHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

    public async Task<Result> Handle(ClearCompletedDownloadTasksCommand command, CancellationToken cancellationToken)
    {
        if (command.DownloadTaskIds != null && command.DownloadTaskIds.Any())
        {
            var downloadTasks = await _dbContext.DownloadTasks
                .Where(x => command.DownloadTaskIds.Contains(x.Id) && x.DownloadStatus == DownloadStatus.Completed)
                .ToListAsync(cancellationToken);
            _dbContext.DownloadTasks.RemoveRange(downloadTasks);
        }
        else
        {
            var downloadTasks = await _dbContext.DownloadTasks.AsTracking()
                .Where(x => x.DownloadStatus == DownloadStatus.Completed)
                .ToListAsync(cancellationToken);
            _dbContext.DownloadTasks.RemoveRange(downloadTasks);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}