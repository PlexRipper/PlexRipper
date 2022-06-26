using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
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
    public GetDownloadTasksByPlexServerIdQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

    public async Task<Result<List<DownloadTask>>> Handle(GetDownloadTasksByPlexServerIdQuery request, CancellationToken cancellationToken)
    {
        var downloadTasks = await PlexServerQueryable
            .AsTracking()
            .IncludeDownloadTasks()
            .FirstOrDefaultAsync(x => x.Id == request.PlexServerId, cancellationToken);

        return Result.Ok(downloadTasks.DownloadTasks.Where(x => x.ParentId == null).ToList());
    }
}