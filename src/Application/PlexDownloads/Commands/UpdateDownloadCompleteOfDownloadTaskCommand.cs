using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.Base;
using PlexRipper.Domain;
using PlexRipper.Domain.Enums;

namespace PlexRipper.Application.PlexDownloads.Commands
{
    public class UpdateDownloadCompleteOfDownloadTaskCommand : IRequest<Result<bool>>
    {
        public UpdateDownloadCompleteOfDownloadTaskCommand(int downloadTaskId, long dataReceived, long dataTotal)
        {
            DownloadTaskId = downloadTaskId;
            DataReceived = dataReceived;
            DataTotal = dataTotal;
        }

        public int DownloadTaskId { get; }
        public long DataReceived { get; }
        public long DataTotal { get; }
    }

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
        public UpdateDownloadCompleteOfDownloadTaskCommandHandler(IPlexRipperDbContext dbContext): base(dbContext) { }

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