using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.PlexDownloads;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.PlexDownloads
{
    public class UpdateDownloadTaskByIdCommandValidator : AbstractValidator<UpdateDownloadTaskByIdCommand>
    {
        public UpdateDownloadTaskByIdCommandValidator()
        {
            RuleFor(x => x.DownloadTask).NotNull();
            RuleFor(x => x.DownloadTask.Id).GreaterThan(0);
            RuleFor(x => x.DownloadTask.DataTotal).GreaterThan(0);
            RuleFor(x => x.DownloadTask.PlexServerId).GreaterThan(0);
            RuleFor(x => x.DownloadTask.PlexLibraryId).GreaterThan(0);
        }
    }

    public class UpdateDownloadTaskByIdCommandHandler : BaseHandler,
        IRequestHandler<UpdateDownloadTaskByIdCommand, Result<bool>>
    {
        public UpdateDownloadTaskByIdCommandHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<bool>> Handle(UpdateDownloadTaskByIdCommand command, CancellationToken cancellationToken)
        {
            var downloadTask = await _dbContext.DownloadTasks.AsTracking()
                .FirstOrDefaultAsync(x => x.Id == command.DownloadTask.Id, cancellationToken);

            if (downloadTask == null)
            {
                return ResultExtensions.GetEntityNotFound(nameof(downloadTask), command.DownloadTask.Id);
            }

            _dbContext.Entry(downloadTask).CurrentValues.SetValues(command.DownloadTask);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok(true);
        }
    }
}