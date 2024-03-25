using PlexRipper.Domain;
using Riok.Mapperly.Abstractions;

namespace Application.Contracts;

[Mapper]
public static partial class PlexMediaSlimDTOMapper
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
    [MapperIgnoreTarget(nameof(PlexMediaSlimDTO.Index))]
    public static partial PlexMediaSlimDTO ToSlimDTO(this PlexMovie plexMovie);

    #endregion

    #region PlexTvShow

    public static PlexMediaSlimDTO ToSlimDTO(this PlexTvShow plexTvShow)
    {
        var dto = plexTvShow.ToSlimDTOMapper();
        dto.Children = new List<PlexMediaSlimDTO>();
        if (plexTvShow.Seasons is not null)
        {
            foreach (var tvShowSeason in plexTvShow.Seasons)
                dto.Children.Add(tvShowSeason.ToSlimDTO());
        }

        return dto;
    }

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(PlexTvShow.FullThumbUrl), nameof(PlexMediaSlimDTO.FullThumbUrl))]
    [MapperIgnoreTarget(nameof(PlexMediaSlimDTO.Index))]
    [MapperIgnoreTarget(nameof(PlexMediaSlimDTO.Children))]
    private static partial PlexMediaSlimDTO ToSlimDTOMapper(this PlexTvShow plexTvShow);

    #endregion

    #region PlexSeason

    public static PlexMediaSlimDTO ToSlimDTO(this PlexTvShowSeason plexTvShowSeason)
    {
        var dto = plexTvShowSeason.ToSlimDTOMapper();
        dto.Children = new List<PlexMediaSlimDTO>();
        if (plexTvShowSeason.Episodes is not null)
        {
            foreach (var episode in plexTvShowSeason.Episodes)
                dto.Children.Add(episode.ToSlimDTO());
        }

        return dto;
    }

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(PlexTvShowSeason.FullThumbUrl), nameof(PlexMediaSlimDTO.FullThumbUrl))]
    [MapperIgnoreTarget(nameof(PlexMediaSlimDTO.Index))]
    [MapperIgnoreTarget(nameof(PlexMediaSlimDTO.Children))]
    [MapperIgnoreSource(nameof(PlexTvShowSeason.Episodes))]
    private static partial PlexMediaSlimDTO ToSlimDTOMapper(this PlexTvShowSeason plexTvShowSeason);

    #endregion

    #region PlexEpisode

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(PlexTvShowEpisode.FullThumbUrl), nameof(PlexMediaSlimDTO.FullThumbUrl))]
    [MapperIgnoreTarget(nameof(PlexMediaSlimDTO.Index))]
    [MapperIgnoreTarget(nameof(PlexMediaSlimDTO.Children))]
    public static partial PlexMediaSlimDTO ToSlimDTO(this PlexTvShowEpisode plexTvShowEpisode);

    #endregion
}