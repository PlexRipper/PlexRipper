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
    public class GetPlexLibraryByIdWithMediaQuery : IRequest<Result<PlexLibrary>>
    {
        public GetPlexLibraryByIdWithMediaQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }

    public class GetPlexLibraryByIdWithMediaQueryValidator : AbstractValidator<GetPlexLibraryByIdWithMediaQuery>
    {
        public GetPlexLibraryByIdWithMediaQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }

    }


    public class GetPlexLibraryByIdWithMediaHandler : BaseHandler, IRequestHandler<GetPlexLibraryByIdWithMediaQuery, Result<PlexLibrary>>
    {
        private readonly IPlexRipperDbContext _dbContext;

        public GetPlexLibraryByIdWithMediaHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<PlexLibrary>> Handle(GetPlexLibraryByIdWithMediaQuery request, CancellationToken cancellationToken)
        {
            var result = await ValidateAsync<GetPlexLibraryByIdWithMediaQuery, GetPlexLibraryByIdWithMediaQueryValidator>(request);
            if (result.IsFailed) return result;

            var plexLibrary = await _dbContext.PlexLibraries
                .Include(x => x.PlexServer)
                .Include(x => x.Movies)
                .Include(x => x.TvShows)
                .FirstOrDefaultAsync(x => x.Id == request.Id);

            return ReturnResult(plexLibrary, request.Id);
        }
    }
}
