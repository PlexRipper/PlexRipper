using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application.PlexMedia;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.CQRS.PlexMedia
{
    public class GetPlexMediaByKeyValidator : AbstractValidator<GetPlexMediaIdByKeyQuery>
    {
        public GetPlexMediaByKeyValidator()
        {
            RuleFor(x => x.Key).GreaterThan(0);
            RuleFor(x => x.MediaType).NotEqual(PlexMediaType.Unknown);
            RuleFor(x => x.MediaType).NotEqual(PlexMediaType.None);
            RuleFor(x => x.PlexServerId).GreaterThan(0);
        }
    }

    public class GetPlexMediaByKeyHandler : BaseHandler, IRequestHandler<GetPlexMediaIdByKeyQuery, Result<int>>
    {
        public GetPlexMediaByKeyHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<int>> Handle(GetPlexMediaIdByKeyQuery request, CancellationToken cancellationToken)
        {

            switch (request.MediaType)
            {
                case PlexMediaType.Movie:
                {
                    var entity = await _dbContext.PlexMovies
                        .FirstOrDefaultAsync(x => x.Key == request.Key &&
                                                  x.PlexServerId == request.PlexServerId, cancellationToken);
                    if (entity is not null)
                    {
                        return Result.Ok(entity.Id);
                    }

                    break;
                }
                case PlexMediaType.TvShow:
                {
                    var entity = await _dbContext.PlexTvShows
                        .FirstOrDefaultAsync(x => x.Key == request.Key &&
                                                  x.PlexServerId == request.PlexServerId, cancellationToken);
                    if (entity is not null)
                    {
                        return Result.Ok(entity.Id);
                    }

                    break;
                }
                case PlexMediaType.Season:
                {
                    var entity = await _dbContext.PlexTvShowSeason
                        .FirstOrDefaultAsync(x => x.Key == request.Key &&
                                                  x.PlexServerId == request.PlexServerId, cancellationToken);
                    if (entity is not null)
                    {
                        return Result.Ok(entity.Id);
                    }

                    break;
                }
                case PlexMediaType.Episode:
                {
                    var entity = await _dbContext.PlexTvShowEpisodes
                        .FirstOrDefaultAsync(x => x.Key == request.Key &&
                                                  x.PlexServerId == request.PlexServerId, cancellationToken);
                    if (entity is not null)
                    {
                        return Result.Ok(entity.Id);
                    }

                    break;
                }
                default:
                    return Result.Fail($"Type {request.MediaType} is not supported for retrieving the plexMediaId by key");
            }

            return Result.Fail(
                $"Couldn't find a plexMediaId with key {request.Key}, plexServerId {request.PlexServerId} with type {request.MediaType}");
        }
    }
}