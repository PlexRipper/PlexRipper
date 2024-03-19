using PlexRipper.Domain;
using Riok.Mapperly.Abstractions;

namespace Application.Contracts;

[Mapper]
public static partial class PlexMediaMapper
{
    #region MediaData

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial PlexMediaDataDTO ToDTO(this PlexMediaData plexMediaSlim);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial PlexMediaQualityDTO ToDTO(this PlexMediaQuality plexMediaSlim);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial PlexMediaDataPartDTO ToDTO(this PlexMediaDataPart plexMediaSlim);

    #endregion

    #region PlexMovie

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(PlexMovie.MovieData), nameof(PlexMediaDTO.MediaData))]
    public static partial PlexMediaDTO ToDTO(this PlexMovie plexMovie);

    public static partial IQueryable<PlexMediaSlimDTO> ProjectToDTO(this IQueryable<PlexMovie> plexMovie);

    #endregion

    #region PlexTvShow

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(PlexTvShow.FullThumbUrl), nameof(PlexMediaDTO.ThumbUrl))]
    [MapProperty(nameof(PlexTvShow.Id), nameof(PlexMediaDTO.TvShowId))]
    [MapProperty(nameof(PlexTvShow.Seasons), nameof(PlexMediaDTO.Children))]
    [MapperIgnoreTarget(nameof(PlexMediaDTO.TvShowSeasonId))]
    [MapperIgnoreTarget(nameof(PlexMediaDTO.MediaData))]
    public static partial PlexMediaDTO ToDTO(this PlexTvShow plexTvShow);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(PlexTvShow.FullThumbUrl), nameof(PlexMediaSlimDTO.ThumbUrl))]
    [MapProperty(nameof(PlexTvShow.Seasons), nameof(PlexMediaSlimDTO.Children))]
    [MapperIgnoreTarget(nameof(PlexMediaSlimDTO.Index))]
    public static partial PlexMediaSlimDTO ToSlimDTO(this PlexTvShow plexTvShow);

    public static partial IQueryable<PlexMediaSlimDTO> ProjectToDTO(this IQueryable<PlexTvShow> plexTvShow);

    #endregion

    #region PlexSeason

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(PlexTvShowSeason.Id), nameof(PlexMediaDTO.TvShowSeasonId))]
    [MapProperty(nameof(PlexTvShowSeason.TvShowId), nameof(PlexMediaDTO.TvShowId))]
    [MapProperty(nameof(PlexTvShowSeason.Episodes), nameof(PlexMediaDTO.Children))]
    [MapperIgnoreTarget(nameof(PlexMediaDTO.MediaData))]
    public static partial PlexMediaDTO ToDTO(this PlexTvShowSeason plexMediaSlim);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(PlexTvShowSeason.Episodes), nameof(PlexMediaSlimDTO.Children))]
    [MapProperty(nameof(PlexTvShowSeason.FullThumbUrl), nameof(PlexMediaSlimDTO.ThumbUrl))]
    [MapperIgnoreTarget(nameof(PlexMediaSlimDTO.Index))]
    public static partial PlexMediaSlimDTO ToSlimDTO(this PlexTvShowSeason plexMediaSlim);

    #endregion

    #region PlexEpisode

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(PlexTvShowEpisode.TvShowId), nameof(PlexMediaDTO.TvShowId))]
    [MapProperty(nameof(PlexTvShowEpisode.TvShowSeasonId), nameof(PlexMediaDTO.TvShowSeasonId))]
    [MapProperty(nameof(PlexTvShowEpisode.EpisodeData), nameof(PlexMediaDTO.MediaData))]
    [MapperIgnoreTarget(nameof(PlexMediaDTO.Children))]
    public static partial PlexMediaDTO ToDTO(this PlexTvShowEpisode plexMediaSlim);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(PlexTvShowEpisode.FullThumbUrl), nameof(PlexMediaSlimDTO.ThumbUrl))]
    [MapperIgnoreTarget(nameof(PlexMediaSlimDTO.Children))]
    [MapperIgnoreTarget(nameof(PlexMediaSlimDTO.Index))]
    public static partial PlexMediaSlimDTO ToSlimDTO(this PlexTvShowEpisode plexMediaSlim);

    #endregion
}