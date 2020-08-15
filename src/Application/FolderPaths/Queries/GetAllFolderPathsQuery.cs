using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Base;
using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
        private readonly IPlexRipperDbContext _dbContext;

        public GetAllFolderPathsQueryHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<List<FolderPath>>> Handle(GetAllFolderPathsQuery request, CancellationToken cancellationToken)
        {
            var folderPaths = await _dbContext.FolderPaths.ToListAsync(cancellationToken);
            return ReturnResult(folderPaths);

        }
    }
}
