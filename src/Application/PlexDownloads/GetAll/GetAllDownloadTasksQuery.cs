using Data.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record GetAllDownloadTasksQuery : IRequest<Result<List<DownloadTask>>> { }

public class GetAllDownloadTasksQueryValidator : AbstractValidator<GetAllDownloadTasksQuery> { }

public class GetAllDownloadTasksQueryHandler : IRequestHandler<GetAllDownloadTasksQuery, Result<List<DownloadTask>>>
{
    private readonly IPlexRipperDbContext _dbContext;

    public GetAllDownloadTasksQueryHandler(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<DownloadTask>>> Handle(GetAllDownloadTasksQuery request, CancellationToken cancellationToken)
    {
        // AsTracking due to Children->Parent cycle error, therefore all navigation properties are added as well
        var downloadList = await _dbContext.DownloadTasks
            .AsTracking()
            .IncludeDownloadTasks()
            .Where(x => x.ParentId == null) // Where clause is to retrieve only the root DownloadTasks
            .ToListAsync(cancellationToken);
        return Result.Ok(downloadList);
    }
}