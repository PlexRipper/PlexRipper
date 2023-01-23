using Data.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;

namespace PlexRipper.Data;

public class GetDownloadTaskByMediaKeyQueryValidator : AbstractValidator<GetDownloadTaskByMediaKeyQuery>
{
    public GetDownloadTaskByMediaKeyQueryValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
        RuleFor(x => x.MediaKey).GreaterThan(0);
    }
}

public class GetDownloadTaskByMediaKeyQueryHandler : BaseHandler, IRequestHandler<GetDownloadTaskByMediaKeyQuery, Result<DownloadTask>>
{
    public GetDownloadTaskByMediaKeyQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

    public async Task<Result<DownloadTask>> Handle(GetDownloadTaskByMediaKeyQuery request, CancellationToken cancellationToken)
    {
        var downloadTask =
            await DownloadTasksQueryable
                .AsTracking()
                .IncludeDownloadTasks()
                .FirstOrDefaultAsync(x => x.PlexServerId == request.PlexServerId && x.Key == request.MediaKey, cancellationToken);

        return ReturnResult(downloadTask, request.MediaKey);
    }
}