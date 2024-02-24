using Data.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexApi.Contracts;

namespace PlexRipper.Application;

public record GetThumbnailImageQuery(int MediaId, PlexMediaType MediaType, int Width = 0, int Height = 0) : IRequest<Result<byte[]>>;

public class GetThumbnailImageQueryValidator : AbstractValidator<GetThumbnailImageQuery>
{
    public GetThumbnailImageQueryValidator()
    {
        RuleFor(x => x.MediaId).GreaterThan(0);
        RuleFor(x => x.MediaType).Must(x => x is PlexMediaType.Movie or PlexMediaType.TvShow);
    }
}

public class GetThumbnailImageQueryHandler : IRequestHandler<GetThumbnailImageQuery, Result<byte[]>>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IPlexApiService _plexServiceApi;

    public GetThumbnailImageQueryHandler(IPlexRipperDbContext dbContext, IPlexApiService plexServiceApi)
    {
        _dbContext = dbContext;
        _plexServiceApi = plexServiceApi;
    }

    public async Task<Result<byte[]>> Handle(GetThumbnailImageQuery command, CancellationToken cancellationToken)
    {
        if (command.MediaType == PlexMediaType.Movie)
        {
            var plexMovie = await _dbContext.PlexMovies.AsQueryable()
                .Include(x => x.PlexServer)
                .GetAsync(command.MediaId, cancellationToken);

            if (plexMovie is null)
                return ResultExtensions.EntityNotFound(nameof(PlexMovie), command.MediaId).LogError();

            if (!plexMovie.HasThumb)
                return Result.Fail($"Movie: {plexMovie.Title} has no thumbnail.");

            return await _plexServiceApi.GetPlexMediaImageAsync(plexMovie.PlexServer, plexMovie.ThumbUrl, command.Width, command.Height, cancellationToken);
        }

        if (command.MediaType == PlexMediaType.TvShow)
        {
            var tvShow = await _dbContext.PlexTvShows.AsQueryable()
                .Include(x => x.PlexServer)
                .GetAsync(command.MediaId, cancellationToken);

            if (tvShow is null)
                return ResultExtensions.EntityNotFound(nameof(PlexTvShow), command.MediaId).LogError();

            if (!tvShow.HasThumb)
                return Result.Fail($"TvShow: {tvShow.Title} has no thumbnail.");

            return await _plexServiceApi.GetPlexMediaImageAsync(tvShow.PlexServer, tvShow.ThumbUrl, command.Width, command.Height, cancellationToken);
        }

        return Result.Fail($"MediaType: {command.MediaType} is not supported when retrieving thumbnails.");
    }
}