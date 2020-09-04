using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.Base;

namespace PlexRipper.Application.PlexDownloads.Queries
{
    public class GetAllDownloadTasksQuery : IRequest<Result<List<DownloadTask>>>
    {
        public GetAllDownloadTasksQuery(bool includeServer = false, bool includeFolderPaths = false, bool includePlexAccount = false,
            bool includePlexLibrary = false)
        {
            IncludeServer = includeServer;
            IncludeFolderPaths = includeFolderPaths;
            IncludePlexAccount = includePlexAccount;
            IncludePlexLibrary = includePlexLibrary;
        }

        public bool IncludeServer { get; }
        public bool IncludeFolderPaths { get; }
        public bool IncludePlexAccount { get; }
        public bool IncludePlexLibrary { get; }
    }

    public class GetAllDownloadTasksQueryValidator : AbstractValidator<GetAllDownloadTasksQuery>
    {
        public GetAllDownloadTasksQueryValidator() { }
    }


    public class GetAllDownloadTasksQueryHandler : BaseHandler, IRequestHandler<GetAllDownloadTasksQuery, Result<List<DownloadTask>>>
    {
        public GetAllDownloadTasksQueryHandler(IPlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<List<DownloadTask>>> Handle(GetAllDownloadTasksQuery request, CancellationToken cancellationToken)
        {
            var query = _dbContext.DownloadTasks.AsQueryable();

            if (request.IncludeServer)
            {
                query = query.Include(x => x.PlexServer);
            }

            if (request.IncludeFolderPaths)
            {
                query = query.Include(x => x.DownloadFolder);
                query = query.Include(x => x.DestinationFolder);
            }

            if (request.IncludePlexAccount)
            {
                query = query.Include(x => x.PlexAccount);
            }

            if (request.IncludePlexLibrary)
            {
                query = query.Include(x => x.PlexLibrary);
            }

            var downloadList = await query
                .Include(x => x.DownloadWorkerTasks)
                .ToListAsync(cancellationToken);
            return Result.Ok(downloadList);
        }
    }
}