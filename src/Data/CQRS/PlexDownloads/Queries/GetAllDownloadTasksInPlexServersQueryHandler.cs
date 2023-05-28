using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data;

public class GetAllDownloadTasksInPlexServersQueryValidator : AbstractValidator<GetAllDownloadTasksInPlexServersQuery> { }

public class GetAllDownloadTasksInPlexServersQueryHandler : BaseHandler,
    IRequestHandler<GetAllDownloadTasksInPlexServersQuery, Result<List<PlexServer>>>
{
    public GetAllDownloadTasksInPlexServersQueryHandler(ILog log, PlexRipperDbContext dbContext) : base(log, dbContext) { }

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