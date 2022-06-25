using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data
{
    public class DeleteDownloadWorkerTasksByDownloadTaskIdCommandValidator : AbstractValidator<DeleteDownloadWorkerTasksByDownloadTaskIdCommand>
    {
        public DeleteDownloadWorkerTasksByDownloadTaskIdCommandValidator()
        {
            RuleFor(x => x.DownloadTaskId).GreaterThan(0);
        }
    }

    public class DeleteDownloadWorkerTasksByDownloadTaskIdCommandHandler : BaseHandler,
        IRequestHandler<DeleteDownloadWorkerTasksByDownloadTaskIdCommand, Result>
    {
        public DeleteDownloadWorkerTasksByDownloadTaskIdCommandHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result> Handle(DeleteDownloadWorkerTasksByDownloadTaskIdCommand command, CancellationToken cancellationToken)
        {
            var downloadWorkerTasks = await _dbContext.DownloadWorkerTasks.AsTracking().Where(x => x.DownloadTaskId == command.DownloadTaskId)
                .ToListAsync(cancellationToken);
            if (!downloadWorkerTasks.Any())
            {
                return Result.Fail(
                        $"Could not find any {nameof(DownloadWorkerTask)}s assigned to {nameof(DownloadTask)} with id: {command.DownloadTaskId}")
                    .LogWarning();
            }

            _dbContext.DownloadWorkerTasks.RemoveRange(downloadWorkerTasks);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}