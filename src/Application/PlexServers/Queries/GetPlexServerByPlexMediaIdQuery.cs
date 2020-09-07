using System;
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

namespace PlexRipper.Application.PlexLibraries.Queries
{
    public class GetPlexServerByPlexMediaIdQuery : IRequest<Result<PlexServer>>
    {
        public GetPlexServerByPlexMediaIdQuery(int mediaId, PlexMediaType plexMediaType)
        {
            MediaId = mediaId;
            PlexMediaType = plexMediaType;
        }

        public int MediaId { get; }

        public PlexMediaType PlexMediaType { get; }
    }

    public class GetPlexServerByPlexMediaIdQueryValidator : AbstractValidator<GetPlexServerByPlexMediaIdQuery>
    {
        public GetPlexServerByPlexMediaIdQueryValidator()
        {
            RuleFor(x => x.MediaId).GreaterThan(0);
        }
    }


    public class GetPlexServerByPlexMediaIdQueryHandler : BaseHandler, IRequestHandler<GetPlexServerByPlexMediaIdQuery, Result<PlexServer>>
    {
        public GetPlexServerByPlexMediaIdQueryHandler(IPlexRipperDbContext dbContext)
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
                default:
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