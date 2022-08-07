using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;

namespace PlexRipper.Data;

public class UpdateDownloadStatusOfDownloadTaskCommandValidator : AbstractValidator<UpdateDownloadStatusOfDownloadTaskCommand>
{
    public UpdateDownloadStatusOfDownloadTaskCommandValidator()
    {
        RuleForEach(x => x.DownloadTaskIds).ChildRules(x => x.RuleFor(y => y).GreaterThan(0));
        RuleFor(x => x.DownloadStatus).NotEqual(DownloadStatus.Unknown);
    }
}

public class UpdateDownloadStatusOfDownloadTaskCommandHandler : BaseHandler,
    IRequestHandler<UpdateDownloadStatusOfDownloadTaskCommand, Result>
{
    public UpdateDownloadStatusOfDownloadTaskCommandHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

    public async Task<Result> Handle(UpdateDownloadStatusOfDownloadTaskCommand command, CancellationToken cancellationToken)
    {
        var downloadTasks = await DownloadTasksQueryable
            .AsTracking()
            .Where(x => command.DownloadTaskIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        foreach (var downloadTask in downloadTasks)
            downloadTask.DownloadStatus = command.DownloadStatus;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}