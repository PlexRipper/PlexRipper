using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data.PlexMedia
{
    public class GetPlexMediaByDownloadTaskQueryValidator : AbstractValidator<GetPlexMediaIdByKeyQuery>
    {
        public GetPlexMediaByDownloadTaskQueryValidator()
        {
            RuleFor(x => x.DownloadTask).NotNull();
            RuleFor(x => x.DownloadTask.Key).GreaterThan(0);
            RuleFor(x => x.DownloadTask.MediaType).NotEqual(PlexMediaType.Unknown);
            RuleFor(x => x.DownloadTask.MediaType).NotEqual(PlexMediaType.None);
            RuleFor(x => x.DownloadTask.PlexServerId).GreaterThan(0);
        }
    }

    public class GetPlexMediaByDownloadTaskQueryHandler : BaseHandler, IRequestHandler<GetPlexMediaIdByKeyQuery, Result<int>>
    {
        public GetPlexMediaByDownloadTaskQueryHandler(PlexRipperDbContext dbContext) : base(dbContext) { }

        public async Task<Result<int>> Handle(GetPlexMediaIdByKeyQuery request, CancellationToken cancellationToken)
        {
            var key = request.DownloadTask.Key;
            var plexServerId = request.DownloadTask.PlexServerId;
            var mediaType = request.DownloadTask.MediaType;

            switch (mediaType)
            {
                case PlexMediaType.Movie:
                {
                    var entity = await _dbContext.PlexMovies
                        .FirstOrDefaultAsync(x => x.Key == key && x.PlexServerId == plexServerId, cancellationToken);
                    if (entity is not null)
                    {
                        return Result.Ok(entity.Id);
                    }

                    break;
                }
                case PlexMediaType.TvShow:
                {
                    var entity = await _dbContext.PlexTvShows
                        .FirstOrDefaultAsync(x => x.Key == key && x.PlexServerId == plexServerId, cancellationToken);
                    if (entity is not null)
                    {
                        return Result.Ok(entity.Id);
                    }

                    break;
                }
                case PlexMediaType.Season:
                {
                    var entity = await _dbContext.PlexTvShowSeason
                        .FirstOrDefaultAsync(x => x.Key == key && x.PlexServerId == plexServerId, cancellationToken);
                    if (entity is not null)
                    {
                        return Result.Ok(entity.Id);
                    }

                    break;
                }
                case PlexMediaType.Episode:
                {
                    var entity = await _dbContext.PlexTvShowEpisodes
                        .FirstOrDefaultAsync(x => x.Key == key && x.PlexServerId == plexServerId, cancellationToken);
                    if (entity is not null)
                    {
                        return Result.Ok(entity.Id);
                    }

                    break;
                }
                default:
                    return Result.Fail($"Type {mediaType} is not supported for retrieving the plexMediaId by key");
            }

            return Result.Fail($"Couldn't find a plexMediaId with key {key}, plexServerId {plexServerId} with type {mediaType}");
        }
    }
}