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
    public class GetPlexServerByPlexMediaIdQueryValidator : AbstractValidator<GetPlexServerByPlexMediaIdQuery>
    {
        public GetPlexServerByPlexMediaIdQueryValidator()
        {
            RuleFor(x => x.MediaId).GreaterThan(0);
        }
    }

    public class GetPlexServerByPlexMediaIdQueryHandler : BaseHandler, IRequestHandler<GetPlexServerByPlexMediaIdQuery, Result<PlexServer>>
    {
        public GetPlexServerByPlexMediaIdQueryHandler(PlexRipperDbContext dbContext)
            : base(dbContext) { }

        public async Task<Result<PlexServer>> Handle(GetPlexServerByPlexMediaIdQuery request, CancellationToken cancellationToken)
        {
            PlexServer plexServer = null;
            switch (request.PlexMediaType)
            {
                case PlexMediaType.Movie:
                    var movie = await _dbContext.PlexMovies
                        .Include(x => x.PlexLibrary)
                        .ThenInclude(x => x.PlexServer)
                        .FirstOrDefaultAsync(x => x.Id == request.MediaId, cancellationToken);
                    plexServer = movie?.PlexLibrary?.PlexServer;
                    break;
                case PlexMediaType.TvShow:
                    var tvshow = await _dbContext.PlexTvShows
                        .Include(x => x.PlexLibrary)
                        .ThenInclude(x => x.PlexServer)
                        .FirstOrDefaultAsync(x => x.Id == request.MediaId, cancellationToken);
                    plexServer = tvshow?.PlexLibrary?.PlexServer;
                    break;
                case PlexMediaType.Season:
                    break;
                case PlexMediaType.Episode:
                    var episode = await _dbContext.PlexTvShowEpisodes
                        .Include(x => x.PlexLibrary)
                        .ThenInclude(x => x.PlexServer)
                        .FirstOrDefaultAsync(x => x.Id == request.MediaId, cancellationToken);
                    plexServer = episode?.PlexLibrary?.PlexServer;
                    break;
                case PlexMediaType.Music:
                    break;
                case PlexMediaType.Album:
                    break;
                case PlexMediaType.Unknown:
                    break;
            }

            if (plexServer != null)
            {
                return Result.Ok(plexServer);
            }

            return Result.Fail($"Could not retrieve the PlexServer for mediaId {request.MediaId} with type {request.PlexMediaType}");
        }
    }
}