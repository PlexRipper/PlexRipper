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
    public class UpdateDownloadTaskByIdCommand : IRequest<Result<bool>>
    {
        public UpdateDownloadTaskByIdCommand(DownloadTask downloadTask)
        {
            DownloadTask = downloadTask;
        }

        public DownloadTask DownloadTask { get; }
    }

    public class UpdateDownloadTaskByIdCommandValidator : AbstractValidator<UpdateDownloadTaskByIdCommand>
    {
        public UpdateDownloadTaskByIdCommandValidator()
        {
            RuleFor(x => x.DownloadTask).NotNull();
            RuleFor(x => x.DownloadTask.Id).GreaterThan(0);
            RuleFor(x => x.DownloadTask.DataTotal).GreaterThan(0);
            RuleFor(x => x.DownloadTask.PlexServerId).GreaterThan(0);
            RuleFor(x => x.DownloadTask.PlexLibraryId).GreaterThan(0);
            RuleFor(x => x.DownloadTask.PlexAccountId).GreaterThan(0);
        }
    }

    public class UpdateDownloadTaskByIdCommandHandler : BaseHandler,
        IRequestHandler<UpdateDownloadTaskByIdCommand, Result<bool>>
    {
        public UpdateDownloadTaskByIdCommandHandler(IPlexRipperDbContext dbContext) : base(dbContext) { }

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