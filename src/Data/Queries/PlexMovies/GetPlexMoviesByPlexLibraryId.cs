using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.PlexMovies;
using PlexRipper.Domain;

namespace PlexRipper.Data.Queries.PlexMovies
{
    public class GetPlexMoviesByPlexLibraryIdValidator : AbstractValidator<GetPlexMoviesByPlexLibraryId>
    {
        public GetPlexMoviesByPlexLibraryIdValidator()
        {
            RuleFor(x => x.PlexLibraryId).GreaterThan(0);
        }
    }

    public class GetPlexMoviesByPlexLibraryIdHandler : IRequestHandler<GetPlexMoviesByPlexLibraryId, Result<List<PlexMovie>>>
    {
        private readonly PlexRipperDbContext _dbContext;

        public GetPlexMoviesByPlexLibraryIdHandler(PlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<List<PlexMovie>>> Handle(GetPlexMoviesByPlexLibraryId request, CancellationToken cancellationToken)
        {
            var plexMovies = await _dbContext.PlexMovies
                .Include(x => x.PlexMovieDatas)
                .ThenInclude(x => x.Parts)
                .Where(x => x.PlexLibraryId == request.PlexLibraryId)
                .ToListAsync(cancellationToken);

            return Result.Ok(plexMovies);
        }
    }
}