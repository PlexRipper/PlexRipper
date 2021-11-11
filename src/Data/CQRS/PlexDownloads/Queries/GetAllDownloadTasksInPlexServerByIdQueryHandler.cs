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

namespace PlexRipper.Data
{
    public class GetAllDownloadTasksInPlexServerByIdQueryValidator : AbstractValidator<GetAllDownloadTasksInPlexServerByIdQuery>
    {
        public GetAllDownloadTasksInPlexServerByIdQueryValidator()
        {
            RuleFor(x => x.PlexServerId).GreaterThan(0);
        }
    }

    public class GetAllDownloadTasksInPlexServerByIdQueryHandler : BaseHandler,
        IRequestHandler<GetAllDownloadTasksInPlexServerByIdQuery, Result<List<DownloadTask>>>
    {
        public GetAllDownloadTasksInPlexServerByIdQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<List<DownloadTask>>> Handle(GetAllDownloadTasksInPlexServerByIdQuery request, CancellationToken cancellationToken)
        {
            var downloadTasks = await PlexServerQueryable
                .AsTracking()
                .IncludeDownloadTasks(true)
                .FirstOrDefaultAsync(x => x.Id == request.PlexServerId, cancellationToken);

            return Result.Ok(downloadTasks.DownloadTasks);
        }
    }

}