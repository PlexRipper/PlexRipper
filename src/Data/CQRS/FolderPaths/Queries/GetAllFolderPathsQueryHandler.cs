using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.FolderPaths
{
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
}