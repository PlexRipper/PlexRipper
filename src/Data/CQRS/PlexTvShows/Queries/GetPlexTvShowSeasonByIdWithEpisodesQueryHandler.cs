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
    public class GetPlexTvShowSeasonByIdWithEpisodesQueryValidator : AbstractValidator<GetPlexTvShowSeasonByIdWithEpisodesQuery>
    {
        public GetPlexTvShowSeasonByIdWithEpisodesQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }

    public class GetPlexTvShowSeasonByIdWithEpisodesQueryHandler : BaseHandler,
        IRequestHandler<GetPlexTvShowSeasonByIdWithEpisodesQuery, Result<PlexTvShowSeason>>
    {
        public GetPlexTvShowSeasonByIdWithEpisodesQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<PlexTvShowSeason>> Handle(GetPlexTvShowSeasonByIdWithEpisodesQuery request, CancellationToken cancellationToken)
        {
            var query = PlexTvShowSeasonsQueryable;

            if (request.IncludeLibrary)
            {
                query = query.IncludePlexLibrary();
            }

            if (request.IncludeServer)
            {
                query = query.IncludeServer();
            }

            var plexTvShowSeason = await query
                .Include(x => x.TvShow)
                .Include(x => x.Episodes)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (plexTvShowSeason == null)
            {
                return ResultExtensions.GetEntityNotFound(nameof(PlexTvShowSeason), request.Id);
            }

            return Result.Ok(plexTvShowSeason);
        }
    }
}