using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common.Base;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexDownloads.Queries
{
    public class GetDownloadTaskByIdQuery : IRequest<Result<DownloadTask>>
    {
        public GetDownloadTaskByIdQuery(int id, bool includeServer = false, bool includeFolderPath = false)
        {
            Id = id;
            IncludeServer = includeServer;
            IncludeFolderPath = includeFolderPath;
        }

        public int Id { get; }
        public bool IncludeServer { get; }
        public bool IncludeFolderPath { get; }
    }

    public class GetDownloadTaskByIdQueryValidator : AbstractValidator<GetDownloadTaskByIdQuery>
    {
        public GetDownloadTaskByIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }


    public class GetDownloadTaskByIdQueryHandler : BaseHandler, IRequestHandler<GetDownloadTaskByIdQuery, Result<DownloadTask>>
    {
        public GetDownloadTaskByIdQueryHandler(IPlexRipperDbContext dbContext): base(dbContext) { }

        public async Task<Result<DownloadTask>> Handle(GetDownloadTaskByIdQuery request, CancellationToken cancellationToken)
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

            var downloadTask = await query.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (downloadTask == null)
            {
                return ResultExtensions.GetEntityNotFound(nameof(DownloadTask), request.Id);
            }

            return Result.Ok(downloadTask);
        }
    }
}