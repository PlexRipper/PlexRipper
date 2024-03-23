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

    #endregion

    #region PlexTvShow

    public static PlexMediaDTO ToDTO(this PlexTvShow plexTvShow)
    {
        var dto = plexTvShow.ToDTOMapper();
        dto.Children = new List<PlexMediaDTO>();
        foreach (var tvShowSeason in plexTvShow.Seasons)
            dto.Children.Add(tvShowSeason.ToDTO());
        return dto;
    }

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(PlexTvShow.Id), nameof(PlexMediaDTO.TvShowId))]
    [MapperIgnoreTarget(nameof(PlexMediaDTO.TvShowSeasonId))]
    [MapperIgnoreTarget(nameof(PlexMediaDTO.MediaData))]
    [MapperIgnoreTarget(nameof(PlexMediaSlimDTO.Index))]
    [MapperIgnoreTarget(nameof(PlexMediaSlimDTO.Children))]
    private static partial PlexMediaDTO ToDTOMapper(this PlexTvShow plexTvShow);

    #endregion

    #region PlexSeason
    public static PlexMediaDTO ToDTO(this PlexTvShowSeason plexTvShowSeason)
    {
        var dto = plexTvShowSeason.ToDTOMapper();
        dto.Children = new List<PlexMediaDTO>();
        foreach (var episode in plexTvShowSeason.Episodes)
            dto.Children.Add(episode.ToDTO());
        return dto;
    }

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(PlexTvShowSeason.Id), nameof(PlexMediaDTO.TvShowSeasonId))]
    [MapProperty(nameof(PlexTvShowSeason.TvShowId), nameof(PlexMediaDTO.TvShowId))]
    [MapProperty(nameof(PlexTvShowSeason.Episodes), nameof(PlexMediaDTO.Children))]
    [MapperIgnoreTarget(nameof(PlexMediaDTO.MediaData))]
    private static partial PlexMediaDTO ToDTOMapper(this PlexTvShowSeason plexTvShowSeason);

    #endregion

    #region PlexEpisode

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(PlexTvShowEpisode.TvShowId), nameof(PlexMediaDTO.TvShowId))]
    [MapProperty(nameof(PlexTvShowEpisode.TvShowSeasonId), nameof(PlexMediaDTO.TvShowSeasonId))]
    [MapProperty(nameof(PlexTvShowEpisode.EpisodeData), nameof(PlexMediaDTO.MediaData))]
    [MapperIgnoreTarget(nameof(PlexMediaDTO.Children))]
    public static partial PlexMediaDTO ToDTO(this PlexTvShowEpisode plexMediaSlim);

    #endregion
}