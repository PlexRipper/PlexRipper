using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexApi.Contracts;

namespace PlexRipper.Application;

public record GetBannerImageQuery(int MediaId, PlexMediaType MediaType, int Width = 0, int Height = 0) : IRequest<Result<byte[]>>;

public class GetBannerImageQueryValidator : AbstractValidator<GetBannerImageQuery>
{
    public GetBannerImageQueryValidator()
    {
        RuleFor(x => x.MediaId).GreaterThan(0);
        RuleFor(x => x.MediaType).Must(x => x is PlexMediaType.Movie or PlexMediaType.TvShow);
    }
}

public class GetBannerImageQueryHandler : IRequestHandler<GetBannerImageQuery, Result<byte[]>>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IPlexApiService _plexServiceApi;

    public GetBannerImageQueryHandler(ILog log, IPlexRipperDbContext dbContext, IPlexApiService plexServiceApi)
    {
        _log = log;
        _dbContext = dbContext;
        _plexServiceApi = plexServiceApi;
    }

    public async Task<Result<byte[]>> Handle(GetBannerImageQuery command, CancellationToken cancellationToken)
    {
        if (command.MediaType == PlexMediaType.Movie)
        {
            var plexMovie = await _dbContext.PlexMovies.AsQueryable()
                .Include(x => x.PlexServer)
                .GetAsync(command.MediaId, cancellationToken);

            if (plexMovie is null)
                return ResultExtensions.EntityNotFound(nameof(PlexMovie), command.MediaId).LogError();

            if (!plexMovie.HasBanner)
                return Result.Fail($"Movie: {plexMovie.Title} has no banner.");

            return await _plexServiceApi.GetPlexMediaImageAsync(plexMovie.PlexServer, plexMovie.BannerUrl, command.Width, command.Height, cancellationToken);
        }

        if (command.MediaType == PlexMediaType.TvShow)
        {
            var tvShow = await _dbContext.PlexTvShows.AsQueryable()
                .Include(x => x.PlexServer)
                .GetAsync(command.MediaId, cancellationToken);

            if (tvShow is null)
                return ResultExtensions.EntityNotFound(nameof(PlexTvShow), command.MediaId).LogError();

            if (!tvShow.HasBanner)
                return Result.Fail($"TvShow: {tvShow.Title} has no banner.");

            return await _plexServiceApi.GetPlexMediaImageAsync(tvShow.PlexServer, tvShow.BannerUrl, command.Width, command.Height, cancellationToken);
        }

        return Result.Fail($"MediaType: {command.MediaType} is not supported when retrieving banners.");
    }
}