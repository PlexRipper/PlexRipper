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
    public class GetAllRelatedDownloadTaskIdsValidator : AbstractValidator<GetAllRelatedDownloadTaskIdsQuery>
    {
        public GetAllRelatedDownloadTaskIdsValidator()
        {
            RuleFor(x => x.DownloadTaskIds).NotEmpty();
            RuleForEach(x => x.DownloadTaskIds).ChildRules(x => x.RuleFor(y => y).GreaterThan(0));
        }
    }

    public class GetAllRelatedDownloadTaskIdsHandler : BaseHandler, IRequestHandler<GetAllRelatedDownloadTaskIdsQuery, Result<List<int>>>
    {
        public GetAllRelatedDownloadTaskIdsHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<List<int>>> Handle(GetAllRelatedDownloadTaskIdsQuery request, CancellationToken cancellationToken)
        {
            var downloadTasks = await DownloadTasksQueryable
                .AsTracking()
                .IncludeDownloadTasks()
                .Where(x => request.DownloadTaskIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            // Retrieve all nested downloadTasks and return those that are downloadable
            var downloadTasksIds = downloadTasks
                .Flatten(x => x.Children)
                .Where(x => x.IsDownloadable)
                .GroupBy(x => x.Id)
                .Select(x => x.First().Id)
                .Where(x => !request.DownloadTaskIds.Contains(x))
                .ToList();

            return Result.Ok(downloadTasksIds);
        }
    }
}