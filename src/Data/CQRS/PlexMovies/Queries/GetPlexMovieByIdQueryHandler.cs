using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.PlexMovies;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.PlexMovies
{
    public class GetPlexMovieByIdQueryValidator : AbstractValidator<GetPlexMovieByIdQuery>
    {
        public GetPlexMovieByIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }

    public class GetPlexMovieByIdQueryHandler : BaseHandler, IRequestHandler<GetPlexMovieByIdQuery, Result<PlexMovie>>
    {
        public GetPlexMovieByIdQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<PlexMovie>> Handle(GetPlexMovieByIdQuery request, CancellationToken cancellationToken)
        {
            var query = _dbContext.PlexMovies.AsQueryable();

            if (request.IncludePlexLibrary && !request.IncludePlexServer)
            {
                query = query
                    .Include(x => x.PlexLibrary);
            }

            if (request.IncludePlexLibrary && request.IncludePlexServer)
            {
                query = query
                    .Include(v => v.PlexLibrary)
                    .ThenInclude(x => x.PlexServer);
            }

            var plexMovie = await query.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (plexMovie == null)
            {
                return ResultExtensions.GetEntityNotFound(nameof(PlexMovie), request.Id);
            }

            return Result.Ok(plexMovie);
        }
    }
}