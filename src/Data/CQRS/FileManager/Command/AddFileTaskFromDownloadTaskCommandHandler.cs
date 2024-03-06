using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.FileManager;

public class AddFileTaskFromDownloadTaskCommandValidator : AbstractValidator<AddFileTaskFromDownloadTaskCommand>
{
    public AddFileTaskFromDownloadTaskCommandValidator()
    {
        RuleFor(x => x.DownloadTaskKey).NotNull();
    }
}

public class AddFileTaskFromDownloadTaskCommandHandler : BaseHandler, IRequestHandler<AddFileTaskFromDownloadTaskCommand, Result<int>>
{
    public AddFileTaskFromDownloadTaskCommandHandler(ILog log, PlexRipperDbContext dbContext) : base(log, dbContext) { }

    public async Task<Result<int>> Handle(AddFileTaskFromDownloadTaskCommand command, CancellationToken cancellationToken)
    {
        var downloadTask = await _dbContext.GetDownloadTaskAsync(command.DownloadTaskKey, cancellationToken);
        var fileTask = new DownloadFileTask
        {
            DownloadTaskId = downloadTask.Id,
            CreatedAt = DateTime.UtcNow,
            FilePathsCompressed = string.Join(';', downloadTask.DownloadWorkerTasks.Select(x => x.TempFilePath).ToArray()),
        };

        await _dbContext.FileTasks.AddAsync(fileTask, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _dbContext.Entry(fileTask).GetDatabaseValuesAsync(cancellationToken);

        return Result.Ok(fileTask.Id);
    }
}