using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.PlexLibraries;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.PlexLibraries
{
    public class GetThumbUrlByPlexMediaIdQueryValidator : AbstractValidator<GetThumbUrlByPlexMediaIdQuery>
    {
        public GetThumbUrlByPlexMediaIdQueryValidator()
        {
            RuleFor(x => x.MediaId).GreaterThan(0);
        }
    }

    public class GetThumbUrlByPlexMediaIdQueryHandler : BaseHandler, IRequestHandler<GetThumbUrlByPlexMediaIdQuery, Result<string>>
    {
        public GetThumbUrlByPlexMediaIdQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<string>> Handle(GetThumbUrlByPlexMediaIdQuery request, CancellationToken cancellationToken)
        {
            string thumbUrl = string.Empty;
            switch (request.PlexMediaType)
            {
                case PlexMediaType.Movie:
                    var movie = await _dbContext.PlexMovies
                        .Include(x => x.PlexLibrary)
                        .ThenInclude(x => x.PlexServer)
                        .FirstOrDefaultAsync(x => x.Id == request.MediaId, cancellationToken);
                    thumbUrl = movie?.ThumbUrl ?? string.Empty;
                    break;
                case PlexMediaType.TvShow:
                    var tvShow = await _dbContext.PlexTvShows
                        .Include(x => x.PlexLibrary)
                        .ThenInclude(x => x.PlexServer)
                        .FirstOrDefaultAsync(x => x.Id == request.MediaId, cancellationToken);
                    thumbUrl = tvShow?.ThumbUrl ?? string.Empty;
                    break;
                case PlexMediaType.Season:
                    break;
                case PlexMediaType.Episode:
                    break;
                case PlexMediaType.Music:
                    break;
                case PlexMediaType.Album:
                    break;
                case PlexMediaType.Unknown:
                    break;
            }

            if (!string.IsNullOrEmpty(thumbUrl))
            {
                return Result.Ok(thumbUrl);
            }

            return Result.Fail($"Could not retrieve the thumb url for {request.MediaId} with type {request.PlexMediaType}");
        }
    }
}