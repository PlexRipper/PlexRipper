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
    public class GetAllDownloadTasksQueryValidator : AbstractValidator<GetAllDownloadTasksQuery> { }

    public class GetAllDownloadTasksQueryHandler : BaseHandler, IRequestHandler<GetAllDownloadTasksQuery, Result<List<DownloadTask>>>
    {
        public GetAllDownloadTasksQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<List<DownloadTask>>> Handle(GetAllDownloadTasksQuery request, CancellationToken cancellationToken)
        {
            // AsTracking due to Children->Parent cycle error, therefore all navigation properties are added as well
            var downloadList = await DownloadTasksQueryable
                .AsTracking()
                .IncludeDownloadTasks(true, true)
                .Where(x => x.ParentId == null) // Where clause is to retrieve only the root DownloadTasks
                .ToListAsync(cancellationToken);
            return Result.Ok(downloadList);
        }
    }
}