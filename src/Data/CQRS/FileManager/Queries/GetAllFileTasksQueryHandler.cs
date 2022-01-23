using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.FileManager.Queries;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.FileManager
{
    public class GetAllFileTasksQueryValidator : AbstractValidator<GetAllFileTasksQuery> { }

    public class GetAllFileTasksQueryHandler : BaseHandler, IRequestHandler<GetAllFileTasksQuery, Result<List<DownloadFileTask>>>
    {
        public GetAllFileTasksQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<List<DownloadFileTask>>> Handle(GetAllFileTasksQuery request, CancellationToken cancellationToken)
        {
            var fileTasks = await _dbContext.FileTasks
                .Include(x => x.DownloadTask)
                .ThenInclude(x => x.DownloadWorkerTasks)
                .Include(x => x.DownloadTask)
                .ThenInclude(x => x.DownloadFolder)
                .Include(x => x.DownloadTask)
                .ThenInclude(x => x.DestinationFolder)
                .ToListAsync(cancellationToken);

            return Result.Ok(fileTasks);
        }
    }
}