using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using PlexRipper.Application.FolderPaths.Queries;
using PlexRipper.Data.Common.Base;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.FolderPaths
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
                return ResultExtensions.GetEntityNotFound(nameof(FolderPath), request.Id);
            }

            return Result.Ok(folderPath);
        }
    }
}