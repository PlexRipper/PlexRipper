using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data;

public class UpdateDownloadTaskWithFileMergeProgressByIdCommandValidator : AbstractValidator<UpdateDownloadTaskWithFileMergeProgressByIdCommand>
{
    public UpdateDownloadTaskWithFileMergeProgressByIdCommandValidator() { }
}

public class UpdateDownloadTaskWithFileMergeProgressByIdCommandHandler : BaseHandler,
    IRequestHandler<UpdateDownloadTaskWithFileMergeProgressByIdCommand, Result>
{
    public UpdateDownloadTaskWithFileMergeProgressByIdCommandHandler(ILog log, PlexRipperDbContext dbContext) : base(log, dbContext) { }

    public async Task<Result> Handle(UpdateDownloadTaskWithFileMergeProgressByIdCommand command, CancellationToken cancellationToken)
    {
        var fileMergeProgress = command.FileMergeProgress;

        await _dbContext.DownloadTasks.Where(x => x.Id == fileMergeProgress.DownloadTaskId)
            .ExecuteUpdateAsync(p => p
                .SetProperty(x => x.Percentage, fileMergeProgress.Percentage)
                .SetProperty(x => x.FileTransferSpeed, fileMergeProgress.TransferSpeed)
                .SetProperty(x => x.DataTotal, fileMergeProgress.DataTotal), cancellationToken);

        return Result.Ok();
    }
}