using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Base;
using PlexRipper.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Application.PlexDownloads.Queries
{
    public class GetDownloadTaskByIdQuery : IRequest<Result<DownloadTask>>
    {
        public GetDownloadTaskByIdQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }
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
        private readonly IPlexRipperDbContext _dbContext;

        public GetDownloadTaskByIdQueryHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<DownloadTask>> Handle(GetDownloadTaskByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await ValidateAsync<GetDownloadTaskByIdQuery, GetDownloadTaskByIdQueryValidator>(request);
            if (result.IsFailed) return result;

            var downloadTask = await _dbContext.DownloadTasks
                                    .Include(x => x.PlexServer)
                                    .Include(x => x.FolderPath)
                                    .FirstOrDefaultAsync(x => x.Id == request.Id);
            return ReturnResult(downloadTask, request.Id);

        }
    }
}
