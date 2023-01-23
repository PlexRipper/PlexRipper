using Application.Contracts;
using Data.Contracts;

namespace PlexRipper.Application;

public class PlexMediaService : IPlexMediaService
{
    protected readonly IMediator _mediator;

    protected readonly IPlexApiService _plexServiceApi;

    public PlexMediaService(IMediator mediator, IPlexApiService plexServiceApi)
    {
        _mediator = mediator;
        _plexServiceApi = plexServiceApi;
    }

    public async Task<Result<byte[]>> GetThumbnailImage(int mediaId, PlexMediaType mediaType, int width = 0, int height = 0)
    {
        if (mediaType == PlexMediaType.Movie)
        {
            var movieResult = await _mediator.Send(new GetPlexMovieByIdQuery(mediaId, includeServer: true));
            if (movieResult.IsFailed)
                return movieResult.ToResult();

            if (!movieResult.Value.HasThumb)
                return Result.Fail($"Movie: {movieResult.Value.Title} has no thumbnail.");

            return await _plexServiceApi.GetPlexMediaImageAsync(movieResult.Value.PlexServer, movieResult.Value.ThumbUrl, width, height);
        }

        if (mediaType == PlexMediaType.TvShow)
        {
            var tvShowResult = await _mediator.Send(new GetPlexTvShowByIdQuery(mediaId, true));
            if (tvShowResult.IsFailed)
                return tvShowResult.ToResult();

            if (!tvShowResult.Value.HasThumb)
                return Result.Fail($"TvShow: {tvShowResult.Value.Title} has no thumbnail.");

            return await _plexServiceApi.GetPlexMediaImageAsync(tvShowResult.Value.PlexServer, tvShowResult.Value.ThumbUrl, width, height);
        }

        return Result.Fail($"MediaType: {mediaType} is not supported when retrieving thumbnails.");
    }

    public async Task<Result<byte[]>> GetBannerImage(int mediaId, PlexMediaType mediaType, int width = 0, int height = 0)
    {
        if (mediaType == PlexMediaType.Movie)
        {
            var movieResult = await _mediator.Send(new GetPlexMovieByIdQuery(mediaId, includeServer: true));
            if (movieResult.IsFailed)
                return movieResult.ToResult();

            if (!movieResult.Value.HasBanner)
                return Result.Fail($"Movie: {movieResult.Value.Title} has no banner.");

            return await _plexServiceApi.GetPlexMediaImageAsync(movieResult.Value.PlexServer, movieResult.Value.BannerUrl, width, height);
        }

        if (mediaType == PlexMediaType.TvShow)
        {
            var tvShowResult = await _mediator.Send(new GetPlexTvShowByIdQuery(mediaId, true));
            if (tvShowResult.IsFailed)
                return tvShowResult.ToResult();

            if (!tvShowResult.Value.HasBanner)
                return Result.Fail($"TvShow: {tvShowResult.Value.Title} has no banner.");

            return await _plexServiceApi.GetPlexMediaImageAsync(tvShowResult.Value.PlexServer, tvShowResult.Value.BannerUrl, width, height);
        }

        return Result.Fail($"MediaType: {mediaType} is not supported when retrieving banners.");
    }
}