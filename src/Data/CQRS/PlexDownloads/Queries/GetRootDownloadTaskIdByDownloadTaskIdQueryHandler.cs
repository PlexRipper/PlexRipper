using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data;

public class GetRootDownloadTaskIdByDownloadTaskIdQueryValidator : AbstractValidator<GetRootDownloadTaskIdByDownloadTaskIdQuery>
{
    public GetRootDownloadTaskIdByDownloadTaskIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class GetRootDownloadTaskIdByDownloadTaskIdQueryHandler : BaseHandler,
    IRequestHandler<GetRootDownloadTaskIdByDownloadTaskIdQuery, Result<int>>
{
    public GetRootDownloadTaskIdByDownloadTaskIdQueryHandler(ILog log, PlexRipperDbContext dbContext) : base(log, dbContext) { }

    public async Task<Result<int>> Handle(GetRootDownloadTaskIdByDownloadTaskIdQuery request, CancellationToken cancellationToken)
    {
        // If null, then it must be assumed the downloadTaskId is already rootLevel so we can return that.
        var rootId =
            await DownloadTasksQueryable
                .Where(x => x.Id == request.Id)
                .Select(x => x.RootDownloadTaskId)
                .SingleOrDefaultAsync(cancellationToken);

        return rootId > 0 ? Result.Ok(rootId) : Result.Fail($"Could not find DownloadTask with id {request.Id}");
    }
}