using PlexRipper.Domain;
using Riok.Mapperly.Abstractions;

namespace Application.Contracts;

[Mapper]
public static partial class DownloadPreviewMapper
{
    #region ToDTO

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial DownloadPreviewDTO ToDTO(this DownloadPreview downloadPreview);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial List<DownloadPreviewDTO> ToDTO(this List<DownloadPreview> downloadPreview);

    #endregion

    #region PlexMovie

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(PlexMovie.MediaSize), nameof(DownloadPreview.Size))]
    [MapProperty(nameof(PlexMovie.Type), nameof(DownloadPreview.MediaType))]
    [MapperIgnoreTarget(nameof(DownloadPreview.Children))]
    [MapperIgnoreTarget(nameof(DownloadPreview.TvShowId))]
    [MapperIgnoreTarget(nameof(DownloadPreview.SeasonId))]
    private static partial DownloadPreview ProjectToDownloadPreviewMapper(this PlexMovie plexMovie);

    public static partial IQueryable<DownloadPreview> ProjectToDownloadPreview(this IQueryable<PlexMovie> plexMovie);

    #endregion

    #region PlexTvShow

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(PlexTvShow.MediaSize), nameof(DownloadPreview.Size))]
    [MapProperty(nameof(PlexTvShow.Type), nameof(DownloadPreview.MediaType))]
    [MapperIgnoreTarget(nameof(DownloadPreview.Children))]
    [MapperIgnoreTarget(nameof(DownloadPreview.TvShowId))]
    [MapperIgnoreTarget(nameof(DownloadPreview.SeasonId))]
    private static partial DownloadPreview ProjectToDownloadPreviewMapper(this PlexTvShow plexTvShow);

    public static partial IQueryable<DownloadPreview> ProjectToDownloadPreview(this IQueryable<PlexTvShow> plexTvShow);

    #endregion

    #region PlexSeason

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(PlexTvShowSeason.MediaSize), nameof(DownloadPreview.Size))]
    [MapProperty(nameof(PlexTvShowSeason.Type), nameof(DownloadPreview.MediaType))]
    [MapperIgnoreTarget(nameof(DownloadPreview.SeasonId))]
    [MapperIgnoreTarget(nameof(DownloadPreview.Children))]
    private static partial DownloadPreview ProjectToDownloadPreviewMapper(this PlexTvShowSeason plexSeason);

    public static partial IQueryable<DownloadPreview> ProjectToDownloadPreview(this IQueryable<PlexTvShowSeason> plexSeason);

    #endregion

    #region PlexTvShowEpisode

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(PlexTvShowEpisode.MediaSize), nameof(DownloadPreview.Size))]
    [MapProperty(nameof(PlexTvShowEpisode.Type), nameof(DownloadPreview.MediaType))]
    [MapProperty(nameof(PlexTvShowEpisode.TvShowSeasonId), nameof(DownloadPreview.SeasonId))]
    [MapperIgnoreTarget(nameof(DownloadPreview.Children))]
    private static partial DownloadPreview ProjectToDownloadPreviewMapper(this PlexTvShowEpisode plexEpisode);

    public static partial IQueryable<DownloadPreview> ProjectToDownloadPreview(this IQueryable<PlexTvShowEpisode> plexEpisode);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(PlexTvShowEpisode.TvShowId), nameof(TvShowEpisodeKeyDTO.TvShowId))]
    [MapProperty(nameof(PlexTvShowEpisode.TvShowSeasonId), nameof(TvShowEpisodeKeyDTO.SeasonId))]
    [MapProperty(nameof(PlexTvShowEpisode.Id), nameof(TvShowEpisodeKeyDTO.EpisodeId))]
    private static partial TvShowEpisodeKeyDTO ProjectToEpisodeKey(this PlexTvShowEpisode plexEpisode);

    public static partial IQueryable<TvShowEpisodeKeyDTO> ProjectToEpisodeKey(this IQueryable<PlexTvShowEpisode> plexEpisode);

    #endregion
}