using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.Base;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;

namespace PlexRipper.Application.PlexTvShows.Queries
{
    public class GetPlexTvShowSeasonByIdWithEpisodesQuery : IRequest<Result<PlexTvShowSeason>>
    {
        public GetPlexTvShowSeasonByIdWithEpisodesQuery(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }

    public class GetPlexTvShowSeasonByIdWithEpisodesQueryValidator : AbstractValidator<GetPlexTvShowSeasonByIdWithEpisodesQuery>
    {
        public GetPlexTvShowSeasonByIdWithEpisodesQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }


    public class GetPlexTvShowSeasonByIdWithEpisodesQueryHandler : BaseHandler, IRequestHandler<GetPlexTvShowSeasonByIdWithEpisodesQuery, Result<PlexTvShowSeason>>
    {
        public GetPlexTvShowSeasonByIdWithEpisodesQueryHandler(IPlexRipperDbContext dbContext): base(dbContext) { }

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