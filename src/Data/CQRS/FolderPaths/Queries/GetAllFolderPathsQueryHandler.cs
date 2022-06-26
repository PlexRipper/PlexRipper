using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.FolderPaths;

public class GetAllFolderPathsQueryValidator : AbstractValidator<GetAllFolderPathsQuery> { }

public class GetAllFolderPathsQueryHandler : BaseHandler, IRequestHandler<GetAllFolderPathsQuery, Result<List<FolderPath>>>
{
    public GetAllFolderPathsQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

    public async Task<Result<List<FolderPath>>> Handle(GetAllFolderPathsQuery request, CancellationToken cancellationToken)
    {
        var folderPaths = await _dbContext.FolderPaths.ToListAsync(cancellationToken);
        return Result.Ok(folderPaths);
    }
}