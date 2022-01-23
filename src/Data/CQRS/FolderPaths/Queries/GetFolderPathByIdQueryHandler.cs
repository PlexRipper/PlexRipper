using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using PlexRipper.Application;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.FolderPaths
{
    public class GetFolderPathByIdQueryValidator : AbstractValidator<GetFolderPathByIdQuery>
    {
        public GetFolderPathByIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }

    public class GetFolderPathByIdQueryHandler : BaseHandler, IRequestHandler<GetFolderPathByIdQuery, Result<FolderPath>>
    {
        public GetFolderPathByIdQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<FolderPath>> Handle(GetFolderPathByIdQuery request, CancellationToken cancellationToken)
        {
            var folderPath = await _dbContext.FolderPaths.FindAsync(request.Id);
            if (folderPath == null)
            {
                return ResultExtensions.EntityNotFound(nameof(FolderPath), request.Id);
            }

            return Result.Ok(folderPath);
        }
    }
}