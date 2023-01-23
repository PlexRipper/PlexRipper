using Data.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;

namespace PlexRipper.Data;

public class GetAllDownloadTasksInPlexServersQueryValidator : AbstractValidator<GetAllDownloadTasksInPlexServersQuery> { }

public class GetAllDownloadTasksInPlexServersQueryHandler : BaseHandler,
    IRequestHandler<GetAllDownloadTasksInPlexServersQuery, Result<List<PlexServer>>>
{
    public GetAllDownloadTasksInPlexServersQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

    public async Task<Result<List<PlexServer>>> Handle(GetAllDownloadTasksInPlexServersQuery request, CancellationToken cancellationToken)
    {
        var query = PlexServerQueryable.AsTracking().IncludeDownloadTasks();

        if (request.IncludeServerStatus)
            query = query.Include(x => x.ServerStatus);

        var serverList = await query
            .Where(x => x.PlexLibraries.Any(y => y.DownloadTasks.Any()))
            .ToListAsync(cancellationToken);
        return Result.Ok(serverList);
    }
}