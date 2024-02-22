using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record GetAllFolderPathsQuery : IRequest<Result<List<FolderPath>>> { }

public class GetAllFolderPathsQueryValidator : AbstractValidator<GetAllFolderPathsQuery> { }

public class GetAllFolderPathsQueryHandler : IRequestHandler<GetAllFolderPathsQuery, Result<List<FolderPath>>>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    public GetAllFolderPathsQueryHandler(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public async Task<Result<List<FolderPath>>> Handle(GetAllFolderPathsQuery request, CancellationToken cancellationToken)
    {
        var folderPaths = await _dbContext.FolderPaths.ToListAsync(cancellationToken);
        return Result.Ok(folderPaths);
    }
}