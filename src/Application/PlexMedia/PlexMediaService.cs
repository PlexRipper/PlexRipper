using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public class PlexMediaService : IPlexMediaService
    {
        protected readonly IMediator _mediator;

        protected readonly IPlexAuthenticationService _plexAuthenticationService;

        protected readonly IPlexApiService _plexServiceApi;

        public PlexMediaService(IMediator mediator, IPlexAuthenticationService plexAuthenticationService,
            IPlexApiService plexServiceApi)
        {
            _mediator = mediator;
            _plexAuthenticationService = plexAuthenticationService;
            _plexServiceApi = plexServiceApi;
        }

        public async Task<Result<byte[]>> GetThumbnailImage(int mediaId, PlexMediaType mediaType, int width = 0, int height = 0)
        {
            string thumbPath;
            PlexServer plexServer;

            switch (mediaType)
            {
                case PlexMediaType.Movie:
                {
                    var movieResult = await _mediator.Send(new GetPlexMovieByIdQuery(mediaId, includeServer: true));
                    if (movieResult.IsFailed)
                    {
                        return movieResult.ToResult();
                    }

                    if (!movieResult.Value.HasThumb)
                    {
                        return Result.Fail($"Movie: {movieResult.Value.Title} has no thumbnail.");
                    }

                    thumbPath = movieResult.Value.ThumbUrl;
                    plexServer = movieResult.Value.PlexServer;
                    break;
                }
                case PlexMediaType.TvShow:
                {
                    var tvShowResult = await _mediator.Send(new GetPlexTvShowByIdQuery(mediaId, true));
                    if (tvShowResult.IsFailed)
                    {
                        return tvShowResult.ToResult();
                    }

                    if (!tvShowResult.Value.HasThumb)
                    {
                        return Result.Fail($"TvShow: {tvShowResult.Value.Title} has no thumbnail.");
                    }

                    thumbPath = tvShowResult.Value.ThumbUrl;
                    plexServer = tvShowResult.Value.PlexServer;
                    break;
                }
                default:
                    return Result.Fail($"MediaType: {mediaType} is not supported when retrieving thumbnails.");
            }

            var token = await _plexAuthenticationService.GetPlexServerTokenAsync(plexServer.Id);
            if (token.IsFailed)
            {
                return token.ToResult();
            }

            return await _plexServiceApi.GetPlexMediaImageAsync(plexServer.ServerUrl + thumbPath, token.Value, width, height);
        }

        public async Task<Result<byte[]>> GetBannerImage(int mediaId, PlexMediaType mediaType, int width = 0, int height = 0)
        {
            string bannerPath;
            PlexServer plexServer;

            switch (mediaType)
            {
                case PlexMediaType.Movie:
                {
                    var movieResult = await _mediator.Send(new GetPlexMovieByIdQuery(mediaId, includeServer: true));
                    if (movieResult.IsFailed)
                    {
                        return movieResult.ToResult();
                    }

                    if (!movieResult.Value.HasBanner)
                    {
                        return Result.Fail($"Movie: {movieResult.Value.Title} has no banner.");
                    }

                    bannerPath = movieResult.Value.ThumbUrl;
                    plexServer = movieResult.Value.PlexServer;
                    break;
                }
                case PlexMediaType.TvShow:
                {
                    var tvShowResult = await _mediator.Send(new GetPlexTvShowByIdQuery(mediaId, true));
                    if (tvShowResult.IsFailed)
                    {
                        return tvShowResult.ToResult();
                    }

                    if (!tvShowResult.Value.HasBanner)
                    {
                        return Result.Fail($"TvShow: {tvShowResult.Value.Title} has no banner.");
                    }

                    bannerPath = tvShowResult.Value.ThumbUrl;
                    plexServer = tvShowResult.Value.PlexServer;
                    break;
                }
                default:
                    return Result.Fail($"MediaType: {mediaType} is not supported when retrieving banners.");
            }

            var token = await _plexAuthenticationService.GetPlexServerTokenAsync(plexServer.Id);
            if (token.IsFailed)
            {
                return token.ToResult();
            }

            return await _plexServiceApi.GetPlexMediaImageAsync(plexServer.ServerUrl + bannerPath, token.Value, width, height);
        }
    }
}