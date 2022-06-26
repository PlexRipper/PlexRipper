using FluentValidation;
using PlexRipper.Application.FileManager.Command;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.FileManager
{
    public class AddFileTaskFromDownloadTaskCommandValidator : AbstractValidator<AddFileTaskFromDownloadTaskCommand>
    {
        public AddFileTaskFromDownloadTaskCommandValidator()
        {
            RuleFor(x => x.DownloadTask).NotNull();
            RuleFor(x => x.DownloadTask.Id).GreaterThan(0);
            RuleFor(x => x.DownloadTask.DownloadStatus).Must(x => x is DownloadStatus.Merging or DownloadStatus.Moving);
            RuleFor(x => x.DownloadTask.DownloadWorkerTasks).NotEmpty();
            RuleFor(x => x.DownloadTask.DestinationFolder).NotNull();
            RuleFor(x => x.DownloadTask.DestinationFolderId).GreaterThan(0);
        }
    }

    public class AddFileTaskFromDownloadTaskCommandHandler : BaseHandler, IRequestHandler<AddFileTaskFromDownloadTaskCommand, Result<int>>
    {
        public AddFileTaskFromDownloadTaskCommandHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<int>> Handle(AddFileTaskFromDownloadTaskCommand command, CancellationToken cancellationToken)
        {
            var fileTask = new DownloadFileTask
            {
                DownloadTaskId = command.DownloadTask.Id,
                CreatedAt = DateTime.UtcNow,
                FilePathsCompressed = command.DownloadTask.GetFilePathsCompressed,
            };

            await _dbContext.FileTasks.AddAsync(fileTask);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _dbContext.Entry(fileTask).GetDatabaseValuesAsync(cancellationToken);

            return Result.Ok(fileTask.Id);
        }
    }
}