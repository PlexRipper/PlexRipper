using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.Common.Interfaces.DataAccess;
using PlexRipper.Domain;
using PlexRipper.Domain.Base;
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
        private readonly IPlexRipperDbContext _dbContext;

        public GetPlexTvShowSeasonByIdWithEpisodesQueryHandler(IPlexRipperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<PlexTvShowSeason>> Handle(GetPlexTvShowSeasonByIdWithEpisodesQuery request, CancellationToken cancellationToken)
        {
            var result = await ValidateAsync<GetPlexTvShowSeasonByIdWithEpisodesQuery, GetPlexTvShowSeasonByIdWithEpisodesQueryValidator>(request);
            if (result.IsFailed) return result;
            var plexTvShowSeason = await _dbContext.PlexTvShowSeason
                .Include(x => x.Episodes)
                .OrderBy(x => x.RatingKey)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (plexTvShowSeason == null)
            {
                return result.Set404NotFoundError();
            }

            plexTvShowSeason.Episodes = plexTvShowSeason.Episodes.OrderBy(x => x.RatingKey).ToList();

            return Result.Ok(plexTvShowSeason);
        }
    }
}