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
    public class GetDownloadTaskByRatingKeyQuery : IRequest<Result<DownloadTask>>
    {
        public GetDownloadTaskByRatingKeyQuery(int ratingKey, bool includeServer = false, bool includeFolderPath = false)
        {
            RatingKey = ratingKey;
            IncludeServer = includeServer;
            IncludeFolderPath = includeFolderPath;
        }

        public int RatingKey { get; }
        public bool IncludeServer { get; }
        public bool IncludeFolderPath { get; }
    }

    public class GetDownloadTaskByRatingKeyQueryValidator : AbstractValidator<GetDownloadTaskByRatingKeyQuery>
    {
        public GetDownloadTaskByRatingKeyQueryValidator()
        {
            RuleFor(x => x.RatingKey).GreaterThan(0);
        }
    }


    public class GetDownloadTaskByRatingKeyQueryHandler : BaseHandler, IRequestHandler<GetDownloadTaskByRatingKeyQuery, Result<DownloadTask>>
    {
        public GetDownloadTaskByRatingKeyQueryHandler(IPlexRipperDbContext dbContext): base(dbContext) { }

        public async Task<Result<DownloadTask>> Handle(GetDownloadTaskByRatingKeyQuery request, CancellationToken cancellationToken)
        {
            var query = _dbContext.DownloadTasks.AsQueryable();

            if (request.IncludeServer)
            {
                query = query.Include(x => x.PlexServer);
            }

            if (request.IncludeFolderPath)
            {
                query = query.Include(x => x.DownloadFolder);
                query = query.Include(x => x.DestinationFolder);
            }

            var downloadTask = await query.FirstOrDefaultAsync(x => x.RatingKey == request.RatingKey, cancellationToken);

            if (downloadTask == null)
            {
                return ResultExtensions.Create404NotFoundResult($"Could not find {nameof(downloadTask)} with ratingKey: {request.RatingKey}");
            }

            return Result.Ok(downloadTask);
        }
    }
}