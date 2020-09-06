using FluentResults;
using FluentValidation;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.Base;
using PlexRipper.Domain;

namespace PlexRipper.Application.FolderPaths.Queries
{
    public class GetFolderPathByIdQuery : IRequest<Result<FolderPath>>
    {
        public GetFolderPathByIdQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }

    public class GetFolderPathByIdQueryValidator : AbstractValidator<GetFolderPathByIdQuery>
    {
        public GetFolderPathByIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }


    public class GetFolderPathByIdQueryHandler : BaseHandler, IRequestHandler<GetFolderPathByIdQuery, Result<FolderPath>>
    {
        public GetFolderPathByIdQueryHandler(IPlexRipperDbContext dbContext) : base(dbContext)
        {
        }

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