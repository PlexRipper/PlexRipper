using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data;

public class GetDownloadTasksByPlexServerIdQueryValidator : AbstractValidator<GetDownloadTasksByPlexServerIdQuery>
{
    public GetDownloadTasksByPlexServerIdQueryValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class GetDownloadTasksByPlexServerIdQueryHandler : BaseHandler,
    IRequestHandler<GetDownloadTasksByPlexServerIdQuery, Result<List<DownloadTask>>>
{
    public GetDownloadTasksByPlexServerIdQueryHandler(ILog log, PlexRipperDbContext dbContext) : base(log, dbContext) { }

    public async Task<Result<List<DownloadTask>>> Handle(GetDownloadTasksByPlexServerIdQuery request, CancellationToken cancellationToken)
    {
        var downloadTasks = await _dbContext.PlexServers
            .AsTracking()
            .IncludeDownloadTasks()
            .FirstOrDefaultAsync(x => x.Id == request.PlexServerId, cancellationToken);

        return Result.Ok(downloadTasks.DownloadTasks.Where(x => x.ParentId == null).ToList());
    }
}