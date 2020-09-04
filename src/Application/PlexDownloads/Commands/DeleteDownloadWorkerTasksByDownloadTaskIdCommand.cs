using System.Linq;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.Base;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;

namespace PlexRipper.Application.PlexDownloads.Commands
{
    public class DeleteDownloadWorkerTasksByDownloadTaskIdCommand : IRequest<Result<bool>>
    {
        public int DownloadTaskId { get; }

        public DeleteDownloadWorkerTasksByDownloadTaskIdCommand(int downloadTaskId)
        {
            DownloadTaskId = downloadTaskId;
        }
    }

    public class DeleteDownloadWorkerTasksByDownloadTaskIdCommandValidator : AbstractValidator<DeleteDownloadWorkerTasksByDownloadTaskIdCommand>
    {
        public DeleteDownloadWorkerTasksByDownloadTaskIdCommandValidator()
        {
            RuleFor(x => x.DownloadTaskId).GreaterThan(0);
        }
    }

    public class DeleteDownloadWorkerTasksByDownloadTaskIdCommandHandler : BaseHandler, IRequestHandler<DeleteDownloadWorkerTasksByDownloadTaskIdCommand, Result<bool>>
    {
        public DeleteDownloadWorkerTasksByDownloadTaskIdCommandHandler(IPlexRipperDbContext dbContext): base(dbContext) { }

        public async Task<Result<bool>> Handle(DeleteDownloadWorkerTasksByDownloadTaskIdCommand command, CancellationToken cancellationToken)
        {
            var downloadWorkerTasks = await _dbContext.DownloadWorkerTasks.AsTracking().Where(x => x.DownloadTaskId == command.DownloadTaskId).ToListAsync(cancellationToken);
            if (!downloadWorkerTasks.Any())
            {
                return Result.Fail($"Could not find any {nameof(DownloadWorkerTask)}s assigned to {nameof(DownloadTask)} with id: {command.DownloadTaskId}").LogWarning();
            }

            _dbContext.DownloadWorkerTasks.RemoveRange(downloadWorkerTasks);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok(true);

        }
    }
}
