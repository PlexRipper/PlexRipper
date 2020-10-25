using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.PlexTvShows.Queries;
using PlexRipper.Data.Common.Base;
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
                .Include(x => x.Episodes)
                .OrderBy(x => x.RatingKey)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (plexTvShowSeason == null)
            {
                return ResultExtensions.GetEntityNotFound(nameof(PlexTvShowSeason), request.Id);
            }

            plexTvShowSeason.Episodes = plexTvShowSeason.Episodes.OrderBy(x => x.RatingKey).ToList();

            return Result.Ok(plexTvShowSeason);
        }
    }
}