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

    /// <summary>
    /// Handles the GetDownloadPreviewQuery by generating download previews for movies and TV shows.
    /// </summary>
    public async Task<Result<List<DownloadPreview>>> Handle(
        GetDownloadPreviewQuery request,
        CancellationToken cancellationToken
    )
    {
        var downloadPreviews = new List<DownloadPreview>();
        if (!request.DownloadMedias.Any())
            return Result.Ok(downloadPreviews);

        // Merge and process movie previews
        var moviesPreview = request.DownloadMedias.Merge(PlexMediaType.Movie);
        var movieResult = await CreateMoviePreviews(moviesPreview);
        downloadPreviews.AddRange(movieResult);

        // Process TV show previews (including seasons and episodes)
        var tvShowResult = await CreateTvShowPreviews(request.DownloadMedias, cancellationToken);
        downloadPreviews.AddRange(tvShowResult);

        return Result.Ok(downloadPreviews);
    }

    /// <summary>
    /// Creates download previews for movies, handling both with and without quality selection.
    /// </summary>
    private async Task<IEnumerable<DownloadPreview>> CreateMoviePreviews(List<DownloadMediaDTO> moviesDownloadMedia)
    {
        if (!moviesDownloadMedia.Any() || !moviesDownloadMedia.Any(x => x.MediaIds.Count > 0))
            return [];

        var previews = new List<DownloadPreview>();
        var movieQualities = moviesDownloadMedia.SelectMany(x => x.Qualities).ToList();
        var movieIdsWithQuality = movieQualities.Select(x => x.MediaId).ToHashSet();

        var baseQuery = _dbContext.PlexMovies.AsNoTracking();

        // Fetch movies with specific qualities
        if (movieQualities.Any())
        {
            var movieMediaDataIds = movieQualities.Select(x => x.DataId).ToHashSet();

            var resultWithQualities = await baseQuery
                .Include(x => x.MediaDataList.Where(y => movieMediaDataIds.Contains(y.Id)))
                .ThenInclude(x => x.Parts)
                .Where(x => movieIdsWithQuality.Contains(x.Id))
                .ProjectToDownloadPreview()
                .ToListAsync();

            previews.AddRange(resultWithQualities);
        }

        // Fetch movies without specific qualities
        var movieIds = moviesDownloadMedia.SelectMany(x => x.MediaIds).Except(movieIdsWithQuality).ToHashSet();
        if (movieIds.Any())
        {
            var result = await baseQuery
                .Include(x => x.MediaDataList)
                .ThenInclude(x => x.Parts)
                .Where(x => movieIds.Contains(x.Id))
                .ProjectToDownloadPreview()
                .ToListAsync();

            previews.AddRange(result);
        }

        // TODO: This might need to use a SortTitle
        return previews.OrderByNatural(x => x.Title);
    }

    /// <summary>
    /// Creates download previews for TV shows, including their seasons and episodes, and builds the hierarchy.
    /// </summary>
    private async Task<IEnumerable<DownloadPreview>> CreateTvShowPreviews(
        List<DownloadMediaDTO> downloadMedias,
        CancellationToken cancellationToken
    )
    {
        var tvShowDownloadMedia = downloadMedias.Merge(PlexMediaType.TvShow);
        var seasonDownloadMedia = downloadMedias.Merge(PlexMediaType.Season);
        var episodeDownloadMedia = downloadMedias.Merge(PlexMediaType.Episode);

        // Get all episode keys for the requested TV shows, seasons, and episodes
        var allKeys = await GetEpisodeKeys(
            tvShowDownloadMedia,
            seasonDownloadMedia,
            episodeDownloadMedia,
            cancellationToken
        );

        var tvShowIds = allKeys.Select(x => x.TvShowId).Distinct().ToList();
        var seasonIds = allKeys.Select(x => x.SeasonId).Distinct().ToList();
        var missingEpisodeIds = allKeys.Select(x => x.EpisodeId).Distinct().ToList();

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

        var episodes = await CreateEpisodePreviews(episodeDownloadMedia, allKeys);

        // Build hierarchy: add episodes to seasons, and seasons to TV shows
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

        return tvShows.OrderByNatural(x => x.Title);
    }

    private async Task<IEnumerable<DownloadPreview>> CreateEpisodePreviews(
        List<DownloadMediaDTO> episodeDownloadMedia,
        List<TvShowEpisodeKeyDTO> missingEpisodeIds
    )
    {
        episodeDownloadMedia.Add(
            new DownloadMediaDTO
            {
                MediaIds = missingEpisodeIds.Select(x => x.EpisodeId).ToList(),
                Qualities = missingEpisodeIds.SelectMany(x => x.Quality.ToDTO()).ToList(),
                Type = PlexMediaType.Episode,
                PlexServerId = 0,
                PlexLibraryId = 0,
            }
        );

        if (!episodeDownloadMedia.Any() || !episodeDownloadMedia.Any(x => x.MediaIds.Count > 0))
            return [];

        var previews = new List<DownloadPreview>();
        var episodeQualities = episodeDownloadMedia.SelectMany(x => x.Qualities).ToList();
        var episodeIdsWithQuality = episodeQualities.Select(x => x.MediaId).ToHashSet();

        var baseQuery = _dbContext.PlexTvShowEpisodes.AsNoTracking();

        if (episodeQualities.Any())
        {
            var episodeMediaDataIds = episodeQualities.Select(x => x.DataId).ToHashSet();

            var resultWithQualities = await baseQuery
                .Include(x => x.MediaDataList.Where(y => episodeMediaDataIds.Contains(y.Id)))
                .ThenInclude(x => x.Parts)
                .Where(x => episodeIdsWithQuality.Contains(x.Id))
                .ProjectToDownloadPreview()
                .ToListAsync();

            previews.AddRange(resultWithQualities);
        }

        // Get all the episode ids that do not have qualities
        var episodeIds = episodeDownloadMedia.SelectMany(x => x.MediaIds).Except(episodeIdsWithQuality).ToHashSet();
        if (episodeIds.Any())
        {
            var result = await baseQuery
                .Include(x => x.MediaDataList)
                .ThenInclude(x => x.Parts)
                .Where(x => episodeIds.Contains(x.Id))
                .ProjectToDownloadPreview()
                .ToListAsync();

            previews.AddRange(result);
        }

        // TODO This might need to use a SortTitle
        return previews.OrderByNatural(x => x.Title);
    }

    private async Task<List<TvShowEpisodeKeyDTO>> GetEpisodeKeys(
        List<DownloadMediaDTO> tvShowDownloadMedia,
        List<DownloadMediaDTO> seasonDownloadMedia,
        List<DownloadMediaDTO> episodeDownloadMedia,
        CancellationToken cancellationToken
    )
    {
        var tvShowEpisodeKeys = new List<TvShowEpisodeKeyDTO>();
        // Only fetch if there are any TV show MediaIds
        if (tvShowDownloadMedia.Any(x => x.MediaIds.Count > 0))
        {
            var tvShowIds = tvShowDownloadMedia.SelectMany(x => x.MediaIds).ToHashSet();
            var tvShowEpisodes = await _dbContext
                .PlexTvShows.AsNoTracking()
                .Include(x => x.Seasons)
                .ThenInclude(x => x.Episodes)
                .ThenInclude(x => x.MediaDataList)
                .Where(x => tvShowIds.Contains(x.Id))
                .SelectMany(x =>
                    x.Seasons.SelectMany(y =>
                        y.Episodes.Select(z => new TvShowEpisodeKeyDTO
                        {
                            TvShowId = z.TvShowId,
                            SeasonId = z.TvShowSeasonId,
                            EpisodeId = z.Id,
                            Quality = z.Qualities.PickMediaQuality(),
                        })
                    )
                )
                .ToListAsync(cancellationToken);
        }

        // Get all the episode ids from the seasons
        var seasonEpisodeKeys = new List<TvShowEpisodeKeyDTO>();
        // Only fetch if there are any season MediaIds
        if (seasonDownloadMedia.Any(x => x.MediaIds.Count > 0))
        {
            var seasonIds = seasonDownloadMedia.SelectMany(x => x.MediaIds).ToHashSet();
            var seasonEpisodes = await _dbContext
                .PlexTvShowSeason.AsNoTracking()
                .Include(x => x.Episodes)
                .ThenInclude(x => x.MediaDataList)
                .Where(x => seasonIds.Contains(x.Id))
                .SelectMany(x =>
                    x.Episodes.Select(y => new TvShowEpisodeKeyDTO
                    {
                        TvShowId = y.TvShowId,
                        SeasonId = y.TvShowSeasonId,
                        EpisodeId = y.Id,
                        Quality = y.Qualities.PickMediaQuality(),
                    })
                )
                .ToListAsync(cancellationToken);
        }

        // Get all the episode ids from the episodes
        var episodesKeys = new List<TvShowEpisodeKeyDTO>();
        // Only fetch if there are any episode MediaIds
        if (episodeDownloadMedia.Any(x => x.MediaIds.Count > 0))
        {
            var episodeIds = episodeDownloadMedia.SelectMany(x => x.MediaIds).ToHashSet();
            var episodes = await _dbContext
                .PlexTvShowEpisodes.AsNoTracking()
                .Include(x => x.MediaDataList)
                .Where(x => episodeIds.Contains(x.Id))
                .ProjectToEpisodeKey()
                .ToListAsync(cancellationToken);
        }

        if (!episodesKeys.Any() && !seasonEpisodeKeys.Any() && !tvShowEpisodeKeys.Any())
            return [];

        // Merge all composite keys and remove duplicates by EpisodeId
        return episodesKeys.Concat(seasonEpisodeKeys).Concat(tvShowEpisodeKeys).DistinctBy(x => x.EpisodeId).ToList();
    }
}
