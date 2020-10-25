using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common;
using PlexRipper.Application.PlexDownloads.Commands;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.PlexDownloads
{
    public class UpdateDownloadStatusOfDownloadTaskCommandValidator : AbstractValidator<UpdateDownloadStatusOfDownloadTaskCommand>
    {
        public UpdateDownloadStatusOfDownloadTaskCommandValidator()
        {
            RuleFor(x => x.DownloadTaskId).GreaterThan(0);
            RuleFor(x => x.DownloadStatus).NotEqual(DownloadStatus.Unknown);
        }
    }

    public class UpdateDownloadStatusOfDownloadTaskCommandHandler : BaseHandler,
        IRequestHandler<UpdateDownloadStatusOfDownloadTaskCommand, Result<bool>>
    {
        public UpdateDownloadStatusOfDownloadTaskCommandHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<bool>> Handle(UpdateDownloadStatusOfDownloadTaskCommand command, CancellationToken cancellationToken)
        {
            var downloadTask = await _dbContext.DownloadTasks.AsTracking()
                .FirstOrDefaultAsync(x => x.Id == command.DownloadTaskId, cancellationToken);

            if (downloadTask != null)
            {
                downloadTask.DownloadStatus = command.DownloadStatus;
                await _dbContext.SaveChangesAsync(cancellationToken);
                return Result.Ok(true);
            }

            return ResultExtensions.Create404NotFoundResult();
        }
    }
}