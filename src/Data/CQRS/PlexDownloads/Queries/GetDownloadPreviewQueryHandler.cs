using AutoMapper;
using AutoMapper.QueryableExtensions;
using Data.Contracts;
using DownloadManager.Contracts;
using DownloadManager.Contracts.Extensions;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data;

public class GetDownloadPreviewQueryValidator : AbstractValidator<GetDownloadPreviewQuery>
{
    public GetDownloadPreviewQueryValidator()
    {
        RuleFor(x => x.DownloadMedias.Count).GreaterThan(0);
    }
}

public class GetDownloadPreviewQueryHandler : BaseHandler, IRequestHandler<GetDownloadPreviewQuery, Result<List<DownloadPreviewDTO>>>
{
    private readonly IMapper _mapper;

    public GetDownloadPreviewQueryHandler(ILog log, IMapper mapper, PlexRipperDbContext dbContext) : base(log, dbContext)
    {
        _mapper = mapper;
    }

    public async Task<Result<List<DownloadPreviewDTO>>> Handle(GetDownloadPreviewQuery request, CancellationToken cancellationToken)
    {
        var moviesPreview = request.DownloadMedias.Merge(PlexMediaType.Movie);
        var tvShowPreview = request.DownloadMedias.Merge(PlexMediaType.TvShow);
        var seasonPreview = request.DownloadMedias.Merge(PlexMediaType.Season);
        var episodePreview = request.DownloadMedias.Merge(PlexMediaType.Episode);

        var downloadPreviews = new List<DownloadPreviewDTO>();

        if (moviesPreview.Any() && moviesPreview.Any(x => x.MediaIds.Count > 0))
        {
            var movieIds = moviesPreview.SelectMany(x => x.MediaIds).ToList();
            var result = await _dbContext.PlexMovies.AsNoTracking()
                .Where(x => movieIds.Contains(x.Id))
                .ProjectTo<DownloadPreviewDTO>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
            downloadPreviews.AddRange(result);
        }

        if (tvShowPreview.Any() && tvShowPreview.Any(x => x.MediaIds.Count > 0))
        {
            var tvShowIds = tvShowPreview.SelectMany(x => x.MediaIds).ToList();
            var result = await _dbContext.PlexTvShows.AsNoTracking()
                .Include(x => x.Seasons)
                .ThenInclude(x => x.Episodes)
                .Where(x => tvShowIds.Contains(x.Id))
                .ProjectTo<DownloadPreviewDTO>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
            downloadPreviews.AddRange(result);
        }

        if (seasonPreview.Any() && seasonPreview.Any(x => x.MediaIds.Count > 0))
        {
            var seasonIds = seasonPreview.SelectMany(x => x.MediaIds).ToList();
            var result = await _dbContext.PlexTvShowSeason.AsNoTracking()
                .Include(x => x.Episodes)
                .Where(x => seasonIds.Contains(x.Id))
                .ProjectTo<DownloadPreviewDTO>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
            downloadPreviews.AddRange(result);
        }

        if (episodePreview.Any() && episodePreview.Any(x => x.MediaIds.Count > 0))
        {
            var episodeIds = episodePreview.SelectMany(x => x.MediaIds).ToList();
            var result = await _dbContext.PlexTvShowEpisodes.AsNoTracking()
                .Where(x => episodeIds.Contains(x.Id))
                .ProjectTo<DownloadPreviewDTO>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
            downloadPreviews.AddRange(result);
        }

        return Result.Ok(downloadPreviews);
    }
}