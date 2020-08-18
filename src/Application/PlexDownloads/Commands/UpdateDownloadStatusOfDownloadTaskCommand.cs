using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common.Base;
using PlexRipper.Domain;
using PlexRipper.Domain.Enums;

namespace PlexRipper.Application.PlexDownloads.Commands
{
    public class UpdateDownloadStatusOfDownloadTaskCommand : IRequest<Result<bool>>
    {
        public UpdateDownloadStatusOfDownloadTaskCommand(int downloadTaskId, DownloadStatus downloadStatus)
        {
            DownloadTaskId = downloadTaskId;
            DownloadStatus = downloadStatus;
        }

        public int DownloadTaskId { get; }
        public DownloadStatus DownloadStatus { get; }
    }

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
        public UpdateDownloadStatusOfDownloadTaskCommandHandler(IPlexRipperDbContext dbContext): base(dbContext) { }

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