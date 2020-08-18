using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common.Base;

namespace PlexRipper.Application.PlexLibraries.Queries
{
    public class GetPlexLibraryByIdWithServerQuery : IRequest<Result<PlexLibrary>>
    {
        public GetPlexLibraryByIdWithServerQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }

    public class GetPlexLibraryByIdWithServerQueryValidator : AbstractValidator<GetPlexLibraryByIdWithServerQuery>
    {
        public GetPlexLibraryByIdWithServerQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }


    public class GetPlexLibraryByIdWithServerQueryHandler : BaseHandler, IRequestHandler<GetPlexLibraryByIdWithServerQuery, Result<PlexLibrary>>
    {
        public GetPlexLibraryByIdWithServerQueryHandler(IPlexRipperDbContext dbContext): base(dbContext) { }

        public async Task<Result<PlexLibrary>> Handle(GetPlexLibraryByIdWithServerQuery request, CancellationToken cancellationToken)
        {
            var result = await ValidateAsync<GetPlexLibraryByIdWithServerQuery, GetPlexLibraryByIdWithServerQueryValidator>(request);
            if (result.IsFailed) return result;

            var plexLibrary = await _dbContext.PlexLibraries
                .Include(x => x.PlexServer)
                .FirstOrDefaultAsync(x => x.Id == request.Id);

            return ReturnResult(plexLibrary, request.Id);
        }
    }
}
