using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Base;
using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Application.PlexDownloads.Queries
{
    public class GetAllDownloadTasksQuery : IRequest<Result<List<DownloadTask>>>
    {
        public GetAllDownloadTasksQuery(bool includeServer = false, bool includeFolderPath = false)
        {
            IncludeServer = includeServer;
            IncludeFolderPath = includeFolderPath;
        }

        public bool IncludeServer { get; }
        public bool IncludeFolderPath { get; }
    }

    public class GetAllDownloadTasksQueryValidator : AbstractValidator<GetAllDownloadTasksQuery>
    {
        public GetAllDownloadTasksQueryValidator()
        {
        }
    }


    public class GetAllDownloadTasksQueryHandler : BaseHandler, IRequestHandler<GetAllDownloadTasksQuery, Result<List<DownloadTask>>>
    {
        private readonly IPlexRipperDbContext _dbContext;

        public GetAllDownloadTasksQueryHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
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

            var downloadList = await query.ToListAsync(cancellationToken);
            return Result.Ok(downloadList);
        }
    }
}
