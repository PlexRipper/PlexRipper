using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.Base;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;

namespace PlexRipper.Application.FileManager.Queries
{
    public class GetFileTaskByIdQuery : IRequest<Result<FileTask>>
    {
        public GetFileTaskByIdQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }

    public class GetFileTaskByIdQueryValidator : AbstractValidator<GetFileTaskByIdQuery>
    {
        public GetFileTaskByIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }


    public class GetFileTaskByIdQueryHandler : BaseHandler, IRequestHandler<GetFileTaskByIdQuery, Result<FileTask>>
    {
        public GetFileTaskByIdQueryHandler(IPlexRipperDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Result<FileTask>> Handle(GetFileTaskByIdQuery request, CancellationToken cancellationToken)
        {
            var fileTask = await _dbContext.FileTasks.FindAsync(request.Id);
            if (fileTask == null)
            {
                return ResultExtensions.GetEntityNotFound(nameof(FileTask), request.Id);
            }
            return Result.Ok(fileTask);
        }
    }
}