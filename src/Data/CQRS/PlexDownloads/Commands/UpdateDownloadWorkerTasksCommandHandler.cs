using Data.Contracts;
using FluentValidation;
using PlexRipper.Application;
using PlexRipper.Data.Common;

namespace PlexRipper.Data;

public class UpdateDownloadWorkerTasksCommandValidator : AbstractValidator<UpdateDownloadWorkerTasksCommand>
{
    public UpdateDownloadWorkerTasksCommandValidator()
    {
        RuleFor(x => x.DownloadTasks).NotNull();
        RuleFor(x => x.DownloadTasks.Count).GreaterThan(0);
        RuleForEach(x => x.DownloadTasks)
            .ChildRules(downloadTask =>
            {
                downloadTask.RuleFor(x => x.Id).GreaterThan(0);
                downloadTask.RuleFor(x => x.BytesReceived).GreaterThan(0);
                downloadTask.RuleFor(x => x.DownloadTaskId).GreaterThan(0);
            });
    }
}

public class UpdateDownloadWorkerTasksCommandHandler : BaseHandler,
    IRequestHandler<UpdateDownloadWorkerTasksCommand, Result<bool>>
{
    public UpdateDownloadWorkerTasksCommandHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

    public async Task<Result<bool>> Handle(UpdateDownloadWorkerTasksCommand command, CancellationToken cancellationToken)
    {
        _dbContext.DownloadWorkerTasks.UpdateRange(command.DownloadTasks);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Ok(true);
    }
}