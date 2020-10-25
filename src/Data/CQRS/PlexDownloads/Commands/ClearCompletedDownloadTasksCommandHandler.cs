using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common;
using PlexRipper.Application.PlexDownloads.Commands;
using PlexRipper.Data.Common.Base;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.PlexDownloads
{
    public class ClearCompletedDownloadTasksCommandValidator : AbstractValidator<ClearCompletedDownloadTasksCommand> { }

    public class ClearCompletedDownloadTasksHandler : BaseHandler,
        IRequestHandler<ClearCompletedDownloadTasksCommand, Result<bool>>
    {
        public ClearCompletedDownloadTasksHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

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