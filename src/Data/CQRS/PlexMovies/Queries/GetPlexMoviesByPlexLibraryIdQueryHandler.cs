using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.PlexMovies
{
    public class GetPlexMoviesByPlexLibraryIdValidator : AbstractValidator<GetPlexMoviesByPlexLibraryId>
    {
        public GetPlexMoviesByPlexLibraryIdValidator()
        {
            RuleFor(x => x.PlexLibraryId).GreaterThan(0);
        }
    }

    public class GetPlexMoviesByPlexLibraryIdHandler : BaseHandler, IRequestHandler<GetPlexMoviesByPlexLibraryId, Result<List<PlexMovie>>>
    {
        public GetPlexMoviesByPlexLibraryIdHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<List<PlexMovie>>> Handle(GetPlexMoviesByPlexLibraryId request, CancellationToken cancellationToken)
        {
            var plexMovies = await PlexMoviesQueryable
                .Where(x => x.PlexLibraryId == request.PlexLibraryId)
                .ToListAsync(cancellationToken);

            return Result.Ok(plexMovies);
        }
    }
}