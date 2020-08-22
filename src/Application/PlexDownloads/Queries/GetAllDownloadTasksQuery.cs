using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common.Base;

namespace PlexRipper.Application.PlexDownloads.Queries
{
    public class GetAllDownloadTasksQuery : IRequest<Result<List<DownloadTask>>>
    {
        public GetAllDownloadTasksQuery(bool includeServer = false, bool includeFolderPath = false, bool includePlexAccount = false)
        {
            IncludeServer = includeServer;
            IncludeFolderPath = includeFolderPath;
            IncludePlexAccount = includePlexAccount;
        }

        public bool IncludeServer { get; }
        public bool IncludeFolderPath { get; }
        public bool IncludePlexAccount { get; }
    }

    public class GetAllDownloadTasksQueryValidator : AbstractValidator<GetAllDownloadTasksQuery>
    {
        public GetAllDownloadTasksQueryValidator()
        {
        }
    }


    public class GetAllDownloadTasksQueryHandler : BaseHandler, IRequestHandler<GetAllDownloadTasksQuery, Result<List<DownloadTask>>>
    {
        public GetAllDownloadTasksQueryHandler(IPlexRipperDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Result<List<DownloadTask>>> Handle(GetAllDownloadTasksQuery request, CancellationToken cancellationToken)
        {
            var query = _dbContext.DownloadTasks.AsQueryable();

            if (request.IncludeServer)
            {
                query = query.Include(x => x.PlexServer);
            }

            if (request.IncludeFolderPath)
            {
                query = query.Include(x => x.FolderPath);
            }

            if (request.IncludePlexAccount)
            {
                query = query.Include(x => x.PlexAccount);
            }

            var downloadList = await query.ToListAsync(cancellationToken);
            return Result.Ok(downloadList);
        }
    }
}