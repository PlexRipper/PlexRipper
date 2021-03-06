﻿using System.Linq;
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
    public class ClearCompletedDownloadTasksCommandValidator : AbstractValidator<ClearCompletedDownloadTasksCommand> { }

    public class ClearCompletedDownloadTasksHandler : BaseHandler,
        IRequestHandler<ClearCompletedDownloadTasksCommand, Result<bool>>
    {
        public ClearCompletedDownloadTasksHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<bool>> Handle(ClearCompletedDownloadTasksCommand command,
            CancellationToken cancellationToken)
        {
            if (command.DownloadTaskIds != null && command.DownloadTaskIds.Any())
            {
                var downloadTasks = await _dbContext.DownloadTasks.Where(x => command.DownloadTaskIds.Contains(x.Id) && x._DownloadStatus == DownloadStatus.Completed.ToString()).ToListAsync();
                _dbContext.DownloadTasks.RemoveRange(downloadTasks);
            }
            else
            {
                var downloadTasks = await _dbContext.DownloadTasks.AsTracking()
                    .Where(x => x._DownloadStatus == DownloadStatus.Completed.ToString())
                    .ToListAsync(cancellationToken);
                _dbContext.DownloadTasks.RemoveRange(downloadTasks);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok(true);
        }
    }
}