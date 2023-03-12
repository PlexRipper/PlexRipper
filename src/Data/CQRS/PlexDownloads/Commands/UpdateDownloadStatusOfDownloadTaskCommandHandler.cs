using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
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
    public UpdateDownloadStatusOfDownloadTaskCommandHandler(ILog log, PlexRipperDbContext dbContext) : base(log, dbContext) { }

    public async Task<Result> Handle(UpdateDownloadStatusOfDownloadTaskCommand command, CancellationToken cancellationToken)
    {
        await _dbContext.DownloadTasks
            .Where(x => command.DownloadTaskIds.Contains(x.Id))
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, x => command.DownloadStatus), cancellationToken);

        return Result.Ok();
    }
}