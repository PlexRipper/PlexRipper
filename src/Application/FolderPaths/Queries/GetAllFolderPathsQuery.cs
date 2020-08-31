using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.Base;

namespace PlexRipper.Application.FolderPaths.Queries
{
    public class GetAllFolderPathsQuery : IRequest<Result<List<FolderPath>>>
    {
        public GetAllFolderPathsQuery()
        {
        }

    }

    public class GetAllFolderPathsQueryValidator : AbstractValidator<GetAllFolderPathsQuery>
    {
        public GetAllFolderPathsQueryValidator()
        {
        }
    }


    public class GetAllFolderPathsQueryHandler : BaseHandler, IRequestHandler<GetAllFolderPathsQuery, Result<List<FolderPath>>>
    {
        public GetAllFolderPathsQueryHandler(IPlexRipperDbContext dbContext): base(dbContext) { }

        public async Task<Result<List<FolderPath>>> Handle(GetAllFolderPathsQuery request, CancellationToken cancellationToken)
        {
            var folderPaths = await _dbContext.FolderPaths.ToListAsync(cancellationToken);
            return Result.Ok(folderPaths);
        }
    }
}
