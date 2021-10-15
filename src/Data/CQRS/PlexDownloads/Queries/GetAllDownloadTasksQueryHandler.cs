using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.PlexDownloads
{
    public class GetAllDownloadTasksQueryValidator : AbstractValidator<GetAllDownloadTasksQuery> { }

    public class GetAllDownloadTasksQueryHandler : BaseHandler, IRequestHandler<GetAllDownloadTasksQuery, Result<List<DownloadTask>>>
    {
        public GetAllDownloadTasksQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<List<DownloadTask>>> Handle(GetAllDownloadTasksQuery request, CancellationToken cancellationToken)
        {
            // AsTracking due to Children->Parent cycle error
            var query = DownloadTasksQueryable.AsTracking();

            if (request.IncludeServer)
            {
                query = query.Include(x => x.PlexServer);
            }

            if (request.IncludePlexLibrary)
            {
                query = query.Include(x => x.PlexLibrary);
            }

            if (request.DownloadTaskIds != null && request.DownloadTaskIds.Any())
            {
                query = query.Where(x => request.DownloadTaskIds.Contains(x.Id));
            }

            // Where clause is to retrieve only the root DownloadTasks
            var downloadList = await query
                .IncludeDownloadTasks()
                .Where(x => x.ParentId == null)
                .ToListAsync(cancellationToken);
            return Result.Ok(downloadList);
        }
    }
}