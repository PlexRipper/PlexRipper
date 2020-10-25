using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.PlexServers.Queries;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.PlexServers
{
    public class GetPlexServerByPlexTvShowSeasonIdQueryValidator : AbstractValidator<GetPlexServerByPlexTvShowSeasonIdQuery>
    {
        public GetPlexServerByPlexTvShowSeasonIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }

    public class GetPlexServerByPlexTvShowSeasonIdQueryHandler : BaseHandler,
        IRequestHandler<GetPlexServerByPlexTvShowSeasonIdQuery, Result<PlexServer>>
    {
        public GetPlexServerByPlexTvShowSeasonIdQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<PlexServer>> Handle(GetPlexServerByPlexTvShowSeasonIdQuery request,
            CancellationToken cancellationToken)
        {
            var plexTvShowEpisode = await _dbContext.PlexTvShowSeason
                .Include(x => x.TvShow)
                .ThenInclude(x => x.PlexLibrary)
                .ThenInclude(x => x.PlexServer)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (plexTvShowEpisode == null)
            {
                return ResultExtensions.Create404NotFoundResult();
            }

            var plexServer = plexTvShowEpisode?.TvShow?.PlexLibrary?.PlexServer ?? null;
            if (plexServer == null)
            {
                return ResultExtensions.GetEntityNotFound(nameof(PlexServer), request.Id)
                    .LogError(null, $"Could not retrieve the PlexServer from PlexTvShowSeason with id: {request.Id}");
            }

            return Result.Ok(plexServer);
        }
    }
}