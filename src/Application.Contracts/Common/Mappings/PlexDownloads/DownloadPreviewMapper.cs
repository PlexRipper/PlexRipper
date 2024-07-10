using PlexRipper.Domain;

namespace Application.Contracts;

public static class DownloadPreviewMapper
{
    #region ToDTO

    public static DownloadPreviewDTO ToDTO(this DownloadPreview source) =>
        new()
        {
            Id = source.Id,
            Title = source.Title,
            Size = source.Size,
            ChildCount = source.ChildCount,
            MediaType = source.MediaType,
            Children = source.Children.ToDTO(),
        };

    public static List<DownloadPreviewDTO> ToDTO(this List<DownloadPreview> source) =>
        source.ConvertAll(ToDTO);

    #endregion

    #region PlexMovie

    private static DownloadPreview ProjectToDownloadPreviewMapper(this PlexMovie source) =>
        new()
        {
            Id = source.Id,
            Title = source.Title,
            Size = source.MediaSize,
            ChildCount = source.ChildCount,
            MediaType = source.Type,
            TvShowId = default,
            SeasonId = default,
            Children = [],
        };

    public static IQueryable<DownloadPreview> ProjectToDownloadPreview(
        this IQueryable<PlexMovie> source
    ) => source.Select(x => ProjectToDownloadPreviewMapper(x));

    #endregion

    #region PlexTvShow

    private static DownloadPreview ProjectToDownloadPreviewMapper(this PlexTvShow source) =>
        new()
        {
            Id = source.Id,
            Title = source.Title,
            Size = source.MediaSize,
            ChildCount = source.ChildCount,
            MediaType = source.Type,
            TvShowId = default,
            SeasonId = default,
            Children = [],
        };

    public static IQueryable<DownloadPreview> ProjectToDownloadPreview(
        this IQueryable<PlexTvShow> source
    ) => source.Select(x => ProjectToDownloadPreviewMapper(x));

    #endregion

    #region PlexSeason

    private static DownloadPreview ProjectToDownloadPreviewMapper(this PlexTvShowSeason source) =>
        new()
        {
            Id = source.Id,
            Title = source.Title,
            Size = source.MediaSize,
            ChildCount = source.ChildCount,
            MediaType = source.Type,
            TvShowId = source.TvShowId,
            SeasonId = default,
            Children = [],
        };

    public static IQueryable<DownloadPreview> ProjectToDownloadPreview(
        this IQueryable<PlexTvShowSeason> source
    ) => source.Select(x => ProjectToDownloadPreviewMapper(x));

    #endregion

    #region PlexTvShowEpisode

    private static DownloadPreview ProjectToDownloadPreviewMapper(this PlexTvShowEpisode source) =>
        new()
        {
            Id = source.Id,
            Title = source.Title,
            Size = source.MediaSize,
            ChildCount = source.ChildCount,
            MediaType = source.Type,
            TvShowId = source.TvShowId,
            SeasonId = source.TvShowSeasonId,
            Children = [],
        };

    public static IQueryable<DownloadPreview> ProjectToDownloadPreview(
        this IQueryable<PlexTvShowEpisode> source
    ) => source.Select(x => ProjectToDownloadPreviewMapper(x));

    private static TvShowEpisodeKeyDTO ProjectToEpisodeKey(this PlexTvShowEpisode source) =>
        new()
        {
            TvShowId = source.TvShowId,
            SeasonId = source.TvShowSeasonId,
            EpisodeId = source.Id,
        };

    public static IQueryable<TvShowEpisodeKeyDTO> ProjectToEpisodeKey(
        this IQueryable<PlexTvShowEpisode> source
    ) => source.Select(x => ProjectToEpisodeKey(x));

    #endregion
}
