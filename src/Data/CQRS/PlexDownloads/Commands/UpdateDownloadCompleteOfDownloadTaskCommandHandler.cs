using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.PlexDownloads.Commands;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.PlexDownloads
{
    public class UpdateDownloadCompleteOfDownloadTaskCommandValidator : AbstractValidator<UpdateDownloadCompleteOfDownloadTaskCommand>
    {
        public UpdateDownloadCompleteOfDownloadTaskCommandValidator()
        {
            RuleFor(x => x.DownloadTaskId).GreaterThan(0);
            RuleFor(x => x.DataReceived).GreaterThan(0);
            RuleFor(x => x.DataTotal).GreaterThan(0);
        }
    }

    public class UpdateDownloadCompleteOfDownloadTaskCommandHandler : BaseHandler,
        IRequestHandler<UpdateDownloadCompleteOfDownloadTaskCommand, Result<bool>>
    {
        public UpdateDownloadCompleteOfDownloadTaskCommandHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<bool>> Handle(UpdateDownloadCompleteOfDownloadTaskCommand command, CancellationToken cancellationToken)
        {
            var downloadTask = await _dbContext.DownloadTasks.AsTracking()
                .FirstOrDefaultAsync(x => x.Id == command.DownloadTaskId, cancellationToken);

            if (downloadTask != null)
            {
                downloadTask.DataReceived = command.DataReceived;
                downloadTask.DataTotal = command.DataTotal;
                await _dbContext.SaveChangesAsync(cancellationToken);
                return Result.Ok(true);
            }

            return ResultExtensions.Create404NotFoundResult();
        }
    }
}