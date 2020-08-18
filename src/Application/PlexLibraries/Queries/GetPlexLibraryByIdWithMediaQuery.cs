using System.Linq;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common.Base;
using PlexRipper.Domain;

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
        public GetPlexLibraryByIdWithMediaHandler(IPlexRipperDbContext dbContext): base(dbContext) { }

        public async Task<Result<PlexLibrary>> Handle(GetPlexLibraryByIdWithMediaQuery request, CancellationToken cancellationToken)
        {
            var result = await ValidateAsync<GetPlexLibraryByIdWithMediaQuery, GetPlexLibraryByIdWithMediaQueryValidator>(request);
            if (result.IsFailed) return result;

            var plexLibrary = await _dbContext.PlexLibraries
                .Include(x => x.PlexServer)
                .Include(x => x.Movies)
                .Include(x => x.TvShows)
                .ThenInclude(x => x.Seasons)
                .ThenInclude(x => x.Episodes)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (plexLibrary == null)
            {
                return ResultExtensions.Create404NotFoundResult();
            }

            // Sort Movies
            if (plexLibrary.Movies.Count > 0)
            {
                plexLibrary.Movies = plexLibrary.Movies.OrderByNatural(x => x.Title).ToList();
            }

            // Sort TvShows
            if (plexLibrary.TvShows.Count > 0)
            {
                plexLibrary.TvShows = plexLibrary.TvShows.OrderBy(x => x.Title).ThenBy(y => y.RatingKey).ToList();
                for (int i = 0; i < plexLibrary.TvShows.Count; i++)
                {
                    plexLibrary.TvShows[i].Seasons = plexLibrary.TvShows[i].Seasons.OrderByNatural(x => x.Title).ToList();

                    for (int j = 0; j < plexLibrary.TvShows[i].Seasons.Count; j++)
                    {
                        plexLibrary.TvShows[i].Seasons[j].Episodes = plexLibrary.TvShows[i].Seasons[j].Episodes.OrderBy(x => x.RatingKey).ToList();
                    }
                }
            }

            return ReturnResult(plexLibrary, request.Id);
        }
    }
}