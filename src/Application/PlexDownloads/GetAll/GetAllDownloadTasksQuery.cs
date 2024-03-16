using Data.Contracts;
using FluentValidation;

namespace PlexRipper.Application;

public record GetAllDownloadTasksQuery : IRequest<Result<List<DownloadTaskGeneric>>> { }

public class GetAllDownloadTasksQueryValidator : AbstractValidator<GetAllDownloadTasksQuery> { }

public class GetAllDownloadTasksQueryHandler : IRequestHandler<GetAllDownloadTasksQuery, Result<List<DownloadTaskGeneric>>>
{
    private readonly IPlexRipperDbContext _dbContext;

    public GetAllDownloadTasksQueryHandler(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<DownloadTaskGeneric>>> Handle(GetAllDownloadTasksQuery request, CancellationToken cancellationToken)
    {
        var downloadList = await _dbContext.GetAllDownloadTasksAsync(cancellationToken: cancellationToken);
        return Result.Ok(downloadList);
    }
}