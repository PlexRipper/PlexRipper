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
    public class GetPlexLibraryByIdQuery : IRequest<Result<PlexLibrary>>
    {
        public GetPlexLibraryByIdQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }

    public class GetLibraryByIdQueryValidator : AbstractValidator<GetPlexLibraryByIdQuery>
    {
        public GetLibraryByIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }

    }


    public class GetLibraryByIdHandler : BaseHandler, IRequestHandler<GetPlexLibraryByIdQuery, Result<PlexLibrary>>
    {
        private readonly IPlexRipperDbContext _dbContext;

        public GetLibraryByIdHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<PlexLibrary>> Handle(GetPlexLibraryByIdQuery request, CancellationToken cancellationToken)
        {

            var result = await ValidateAsync<
                GetPlexLibraryByIdQuery,
                GetLibraryByIdQueryValidator>(request);
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
