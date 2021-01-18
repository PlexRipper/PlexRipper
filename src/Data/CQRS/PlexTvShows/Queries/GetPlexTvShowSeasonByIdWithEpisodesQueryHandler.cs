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
            var plexTvShowSeason = await _dbContext.PlexTvShowSeason
                .Include(x => x.TvShow)
                .Include(x => x.PlexLibrary)
                .ThenInclude(x => x.PlexServer)
                .Include(x => x.Episodes)
                .ThenInclude(x => x.EpisodeData)
                .ThenInclude(x => x.Parts)
                .OrderBy(x => x.Key)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (plexTvShowSeason == null)
            {
                return ResultExtensions.GetEntityNotFound(nameof(PlexTvShowSeason), request.Id);
            }

            plexTvShowSeason.Episodes = plexTvShowSeason.Episodes.OrderBy(x => x.Key).ToList();

            return Result.Ok(plexTvShowSeason);
        }
    }
}