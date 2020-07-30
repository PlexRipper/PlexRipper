using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Base;
using PlexRipper.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

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
        private readonly IPlexRipperDbContext _dbContext;

        public GetPlexLibraryByIdWithServerQueryHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

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
