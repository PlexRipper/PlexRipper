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
    public class GetAllRelatedDownloadTasksValidator : AbstractValidator<GetAllRelatedDownloadTasksQuery>
    {
        public GetAllRelatedDownloadTasksValidator()
        {
            RuleFor(x => x.DownloadTaskIds).NotEmpty();
            RuleForEach(x => x.DownloadTaskIds).ChildRules(x => x.RuleFor(y => y).GreaterThan(0));
        }
    }

    public class GetAllRelatedDownloadTasksHandler : BaseHandler, IRequestHandler<GetAllRelatedDownloadTasksQuery, Result<List<DownloadTask>>>
    {
        public GetAllRelatedDownloadTasksHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<List<DownloadTask>>> Handle(GetAllRelatedDownloadTasksQuery request, CancellationToken cancellationToken)
        {
            var downloadTasks = await DownloadTasksQueryable
                .AsTracking()
                .IncludeDownloadTasks()
                .Where(x => request.DownloadTaskIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            var downloadTasksIds = downloadTasks.Flatten(x => x.Children).GroupBy(x => x.Id).Select(x => x.First()).ToList();

            return Result.Ok(downloadTasksIds);
        }
    }
}