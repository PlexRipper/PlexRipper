using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.PlexTvShows;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.PlexTvShows
{
    public class GetPlexTvShowByIdWithEpisodesQueryValidator : AbstractValidator<GetPlexTvShowByIdWithEpisodesQuery>
    {
        public GetPlexTvShowByIdWithEpisodesQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }

    public class GetPlexTvShowByIdWithEpisodesQueryHandler : BaseHandler, IRequestHandler<GetPlexTvShowByIdWithEpisodesQuery, Result<PlexTvShow>>
    {
        public GetPlexTvShowByIdWithEpisodesQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<PlexTvShow>> Handle(GetPlexTvShowByIdWithEpisodesQuery request, CancellationToken cancellationToken)
        {
            IQueryable<PlexTvShow> query = PlexTvShowsQueryable;

            if (!request.IncludeData)
            {
                query = query.Include(x => x.Seasons)
                    .ThenInclude(x => x.Episodes);
            }
            else
            {
                query = query.Include(x => x.Seasons)
                    .ThenInclude(x => x.Episodes);
            }

            if (request.IncludeLibrary)
            {
                query = query
                    .Include(x => x.PlexLibrary)
                    .ThenInclude(x => x.PlexServer);
            }

            var plexTvShow = await query
                .OrderBy(x => x.Key)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (plexTvShow == null)
            {
                return ResultExtensions.EntityNotFound(nameof(PlexTvShow), request.Id);
            }

            plexTvShow.Seasons = plexTvShow.Seasons.OrderBy(x => x.Title).ToList();

            return Result.Ok(plexTvShow);
        }
    }
}