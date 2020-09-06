using System.Linq;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.Base;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexLibraries.Queries
{
    public class GetPlexLibraryByIdQuery : IRequest<Result<PlexLibrary>>
    {
        public GetPlexLibraryByIdQuery(int id, bool includePlexServer = false, bool includeMedia = false)
        {
            Id = id;
            IncludePlexServer = includePlexServer;
            IncludeMedia = includeMedia;
        }

        public int Id { get; }
        public bool IncludePlexServer { get; }
        public bool IncludeMedia { get; }
    }

    public class GetPlexLibraryByIdQueryValidator : AbstractValidator<GetPlexLibraryByIdQuery>
    {
        public GetPlexLibraryByIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }


    public class GetPlexLibraryByIdWithMediaHandler : BaseHandler, IRequestHandler<GetPlexLibraryByIdQuery, Result<PlexLibrary>>
    {
        public GetPlexLibraryByIdWithMediaHandler(IPlexRipperDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Result<PlexLibrary>> Handle(GetPlexLibraryByIdQuery request, CancellationToken cancellationToken)
        {
            var query = _dbContext.PlexLibraries.AsQueryable();

            if (request.IncludePlexServer)
            {
                query = query
                    .Include(x => x.PlexServer);
            }

            if (request.IncludeMedia)
            {
                query = query
                    .Include(x => x.Movies)
                    .Include(x => x.TvShows)
                    .ThenInclude(x => x.Seasons)
                    .ThenInclude(x => x.Episodes);
            }

            var plexLibrary = await query.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (plexLibrary == null)
            {
                return ResultExtensions.GetEntityNotFound(nameof(PlexLibrary), request.Id);
            }

            if (request.IncludeMedia)
            {
                // Sort Movies
                if (plexLibrary.Movies?.Count > 0)
                {
                    plexLibrary.Movies = plexLibrary.Movies.OrderByNatural(x => x.Title).ToList();
                }

                // Sort TvShows
                if (plexLibrary.TvShows?.Count > 0)
                {
                    plexLibrary.TvShows = plexLibrary.TvShows.OrderBy(x => x.Title).ThenBy(y => y.RatingKey).ToList();
                    for (int i = 0; i < plexLibrary.TvShows.Count; i++)
                    {
                        plexLibrary.TvShows[i].Seasons = plexLibrary.TvShows[i].Seasons.OrderByNatural(x => x.Title).ToList();

                        for (int j = 0; j < plexLibrary.TvShows[i].Seasons.Count; j++)
                        {
                            plexLibrary.TvShows[i].Seasons[j].Episodes =
                                plexLibrary.TvShows[i].Seasons[j].Episodes.OrderBy(x => x.RatingKey).ToList();
                        }
                    }
                }
            }

            return Result.Ok(plexLibrary);
        }
    }
}