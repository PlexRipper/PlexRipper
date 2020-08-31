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
using PlexRipper.Domain.Enums;

namespace PlexRipper.Application.PlexDownloads.Commands
{
    public class ClearCompletedDownloadTasksCommand : IRequest<Result<bool>>
    {
        public ClearCompletedDownloadTasksCommand() { }
    }

    public class ClearCompletedDownloadTasksCommandValidator : AbstractValidator<ClearCompletedDownloadTasksCommand>
    {
        public ClearCompletedDownloadTasksCommandValidator() { }
    }

    public class ClearCompletedDownloadTasksHandler : BaseHandler,
        IRequestHandler<ClearCompletedDownloadTasksCommand, Result<bool>>
    {
        public ClearCompletedDownloadTasksHandler(IPlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<bool>> Handle(ClearCompletedDownloadTasksCommand command,
            CancellationToken cancellationToken)
        {
            var downloadTasks = await _dbContext.DownloadTasks.AsTracking()
                .Where(x => x._DownloadStatus == DownloadStatus.Completed.ToString())
                .ToListAsync(cancellationToken);

            _dbContext.DownloadTasks.RemoveRange(downloadTasks);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok(true);
        }
    }
}