using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.PlexTvShows
{
    public class GetPlexTvShowEpisodeByIdQueryValidator : AbstractValidator<GetPlexTvShowEpisodeByIdQuery>
    {
        public GetPlexTvShowEpisodeByIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }

    public class GetPlexTvShowEpisodeByIdQueryHandler : BaseHandler, IRequestHandler<GetPlexTvShowEpisodeByIdQuery, Result<PlexTvShowEpisode>>
    {
        public GetPlexTvShowEpisodeByIdQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<PlexTvShowEpisode>> Handle(GetPlexTvShowEpisodeByIdQuery request, CancellationToken cancellationToken)
        {
            var plexTvShowEpisode = await PlexTvShowEpisodesQueryable
                .Include(x => x.TvShow)
                .Include(x => x.TvShowSeason)
                .ThenInclude(x => x.TvShow)
                .Include(x => x.PlexLibrary)
                .Include(x => x.PlexServer)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (plexTvShowEpisode == null)
            {
                return ResultExtensions.EntityNotFound(nameof(PlexTvShowEpisode), request.Id);
            }

            return Result.Ok(plexTvShowEpisode);
        }
    }
}