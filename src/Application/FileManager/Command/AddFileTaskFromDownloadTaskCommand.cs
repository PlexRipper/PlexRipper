using System;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.Base;
using PlexRipper.Domain;

namespace PlexRipper.Application.FileManager.Command
{
    public class AddFileTaskFromDownloadTaskCommand : IRequest<Result<int>>
    {
        public AddFileTaskFromDownloadTaskCommand(DownloadTask downloadTask)
        {
            DownloadTask = downloadTask;
        }

        public DownloadTask DownloadTask { get; }
    }

    public class AddFileTaskFromDownloadTaskCommandValidator : AbstractValidator<AddFileTaskFromDownloadTaskCommand>
    {
        public AddFileTaskFromDownloadTaskCommandValidator()
        {
            RuleFor(x => x.DownloadTask).NotNull();
            RuleFor(x => x.DownloadTask.Id).GreaterThan(0);
            RuleFor(x => x.DownloadTask.DownloadStatus).IsInEnum();
            RuleFor(x => x.DownloadTask.DownloadStatus).Must(x => x == DownloadStatus.Completed);
            RuleFor(x => x.DownloadTask.DownloadWorkerTasks).NotEmpty();
        }
    }

    public class AddFileTaskFromDownloadTaskCommandHandler : BaseHandler, IRequestHandler<AddFileTaskFromDownloadTaskCommand, Result<int>>
    {
        public AddFileTaskFromDownloadTaskCommandHandler(IPlexRipperDbContext dbContext): base(dbContext) { }

        public async Task<Result<int>> Handle(AddFileTaskFromDownloadTaskCommand command, CancellationToken cancellationToken)
        {
            var fileTask = new FileTask(command.DownloadTask)
            {
                CreatedAt = DateTime.UtcNow,
            };
            await _dbContext.FileTasks.AddAsync(fileTask);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _dbContext.Entry(fileTask).GetDatabaseValuesAsync(cancellationToken);

            return Result.Ok(fileTask.Id);

        }
    }
}
