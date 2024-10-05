using Application.Contracts;
using Data.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record GetDownloadPreviewQuery(List<DownloadMediaDTO> DownloadMedias) : IRequest<Result<List<DownloadPreview>>>;

public class GetDownloadPreviewQueryValidator : AbstractValidator<GetDownloadPreviewQuery>
{
    public GetDownloadPreviewQueryValidator()
    {
        RuleFor(x => x.DownloadMedias.Count).GreaterThan(0);
    }
}

public class GetDownloadPreviewQueryHandler : IRequestHandler<GetDownloadPreviewQuery, Result<List<DownloadPreview>>>
{
    private readonly IPlexRipperDbContext _dbContext;

    public GetDownloadPreviewQueryHandler(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<DownloadPreview>>> Handle(
        GetDownloadPreviewQuery request,
        CancellationToken cancellationToken
    )
    {
        var downloadPreviews = new List<DownloadPreview>();
        if (!request.DownloadMedias.Any())
            return Result.Ok(downloadPreviews);

        var moviesPreview = request.DownloadMedias.Merge(PlexMediaType.Movie);
        var tvShowPreview = request.DownloadMedias.Merge(PlexMediaType.TvShow);
        var seasonPreview = request.DownloadMedias.Merge(PlexMediaType.Season);
        var episodePreview = request.DownloadMedias.Merge(PlexMediaType.Episode);

        if (moviesPreview.Any() && moviesPreview.Any(x => x.MediaIds.Count > 0))
        {
            var movieIds = moviesPreview.SelectMany(x => x.MediaIds).ToList();
            var result = await _dbContext
                .PlexMovies.AsNoTracking()
                .Where(x => movieIds.Contains(x.Id))
                .ProjectToDownloadPreview()
                .ToListAsync(cancellationToken);
            downloadPreviews.AddRange(result.OrderByNatural(x => x.Title));
        }

        // Get all the episode ids from the tv shows
        var tvShowEpisodeKeys = new List<TvShowEpisodeKeyDTO>();
        if (!tvShowPreview.Any() || tvShowPreview.Any(x => x.MediaIds.Count > 0))
        {
            var tvShowPreviewIds = tvShowPreview.SelectMany(x => x.MediaIds).ToList();
            tvShowEpisodeKeys = await _dbContext
                .PlexTvShows.AsNoTracking()
                .Include(x => x.Seasons)
                .ThenInclude(x => x.Episodes)
                .Where(x => tvShowPreviewIds.Contains(x.Id))
                .SelectMany(x =>
                    x.Seasons.SelectMany(y =>
                        y.Episodes.Select(z => new TvShowEpisodeKeyDTO
                        {
                            TvShowId = z.TvShowId,
                            SeasonId = z.TvShowSeasonId,
                            EpisodeId = z.Id,
                        })
                    )
                )
                .ToListAsync(cancellationToken);
        }

        // Get all the episode ids from the seasons
        var seasonEpisodeKeys = new List<TvShowEpisodeKeyDTO>();
        if (!seasonPreview.Any() || seasonPreview.Any(x => x.MediaIds.Count > 0))
        {
            var seasonPreviewIds = seasonPreview.SelectMany(x => x.MediaIds).ToList();
            seasonEpisodeKeys = await _dbContext
                .PlexTvShowSeason.AsNoTracking()
                .Include(x => x.Episodes)
                .Where(x => seasonPreviewIds.Contains(x.Id))
                .SelectMany(x =>
                    x.Episodes.Select(y => new TvShowEpisodeKeyDTO
                    {
                        TvShowId = y.TvShowId,
                        SeasonId = y.TvShowSeasonId,
                        EpisodeId = y.Id,
                    })
                )
                .ToListAsync(cancellationToken);
        }

        // Get all the episode ids from the episodes
        var episodesKeys = new List<TvShowEpisodeKeyDTO>();
        if (!episodePreview.Any() || episodePreview.Any(x => x.MediaIds.Count > 0))
        {
            var episodePreviewIds = episodePreview.SelectMany(x => x.MediaIds).ToList();
            episodesKeys = await _dbContext
                .PlexTvShowEpisodes.AsNoTracking()
                .Where(x => episodePreviewIds.Contains(x.Id))
                .ProjectToEpisodeKey()
                .ToListAsync(cancellationToken);
        }

        if (!episodesKeys.Any() && !seasonEpisodeKeys.Any() && !tvShowEpisodeKeys.Any())
            return Result.Ok(downloadPreviews);

        // Merge all composite keys together and remove duplicates
        var allKeys = episodesKeys
            .Concat(seasonEpisodeKeys)
            .Concat(tvShowEpisodeKeys)
            .DistinctBy(x => x.EpisodeId)
            .ToList();

        var tvShowIds = allKeys.Select(x => x.TvShowId).Distinct().ToList();
        var seasonIds = allKeys.Select(x => x.SeasonId).Distinct().ToList();
        var episodeIds = allKeys.Select(x => x.EpisodeId).Distinct().ToList();

        // Retrieve all the tv shows, seasons and episodes
        var tvShows = await _dbContext
            .PlexTvShows.AsNoTracking()
            .Where(x => tvShowIds.Contains(x.Id))
            .ProjectToDownloadPreview()
            .ToListAsync(cancellationToken);

        var seasons = await _dbContext
            .PlexTvShowSeason.AsNoTracking()
            .Where(x => seasonIds.Contains(x.Id))
            .ProjectToDownloadPreview()
            .ToListAsync(cancellationToken);

        var episodes = await _dbContext
            .PlexTvShowEpisodes.AsNoTracking()
            .Where(x => episodeIds.Contains(x.Id))
            .ProjectToDownloadPreview()
            .ToListAsync(cancellationToken);

        // Build hierarchy
        foreach (var season in seasons)
        {
            var result = episodes.Where(x => x.SeasonId == season.Id).ToList().OrderByNatural(x => x.Title);
            season.Children.AddRange(result);
            season.Size = season.Children.Sum(x => x.Size);
            season.ChildCount = season.Children.Count;
        }

        foreach (var tvShow in tvShows)
        {
            var result = seasons.Where(x => x.TvShowId == tvShow.Id).ToList().OrderByNatural(x => x.Title);
            tvShow.Children.AddRange(result);
            tvShow.Size = tvShow.Children.Sum(x => x.Size);
            tvShow.ChildCount = tvShow.Children.Count;
        }

        downloadPreviews.AddRange(tvShows);

        return Result.Ok(downloadPreviews);
    }
}
