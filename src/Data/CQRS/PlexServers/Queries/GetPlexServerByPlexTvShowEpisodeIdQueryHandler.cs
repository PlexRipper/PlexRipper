using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.PlexServers;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.PlexServers
{
    public class GetPlexServerByPlexTvShowEpisodeIdQueryValidator : AbstractValidator<GetPlexServerByPlexTvShowEpisodeIdQuery>
    {
        public GetPlexServerByPlexTvShowEpisodeIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }

    public class GetPlexServerByPlexTvShowEpisodeIdQueryHandler : BaseHandler,
        IRequestHandler<GetPlexServerByPlexTvShowEpisodeIdQuery, Result<PlexServer>>
    {
        public GetPlexServerByPlexTvShowEpisodeIdQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<PlexServer>> Handle(GetPlexServerByPlexTvShowEpisodeIdQuery request,
            CancellationToken cancellationToken)
        {
            var plexTvShowEpisode = await _dbContext.PlexTvShowEpisodes
                .Include(x => x.TvShowSeason)
                .ThenInclude(x => x.TvShow)
                .ThenInclude(x => x.PlexLibrary)
                .ThenInclude(x => x.PlexServer)
                .ThenInclude(x => x.ServerStatus)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (plexTvShowEpisode == null)
            {
                return ResultExtensions.Create404NotFoundResult();
            }

            var plexServer = plexTvShowEpisode?.TvShowSeason?.TvShow?.PlexLibrary?.PlexServer;
            if (plexServer == null)
            {
                return ResultExtensions.EntityNotFound(nameof(PlexServer), request.Id)
                    .LogError(null, $"Could not retrieve the PlexServer from PlexTvShowEpisode with id: {request.Id}");
            }

            return Result.Ok(plexServer);
        }
    }
}