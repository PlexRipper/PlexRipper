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
    IRequestHandler<UpdateDownloadTaskWithFileMergeProgressByIdCommand, Result<DownloadTask>>
{
    public UpdateDownloadTaskWithFileMergeProgressByIdCommandHandler(ILog log, PlexRipperDbContext dbContext) : base(log, dbContext) { }

    public async Task<Result<DownloadTask>> Handle(UpdateDownloadTaskWithFileMergeProgressByIdCommand command, CancellationToken cancellationToken)
    {
        var fileMergeProgress = command.FileMergeProgress;
        var downloadTaskDb = await _dbContext.DownloadTasks.AsTracking().FirstOrDefaultAsync(x => x.Id == fileMergeProgress.DownloadTaskId, cancellationToken);

        downloadTaskDb.Percentage = fileMergeProgress.Percentage;
        downloadTaskDb.DownloadSpeed = fileMergeProgress.TransferSpeed;
        downloadTaskDb.DataTotal = fileMergeProgress.DataTotal;

        await SaveChangesAsync(cancellationToken);

        return Result.Ok(downloadTaskDb);
    }
}