using AutoMapper;

namespace PlexRipper.Data;

public class DataMappingProfile : Profile
{
    public DataMappingProfile()
    {
        CreateProjection<PlexTvShow, PlexMediaSlim>();
        CreateProjection<PlexTvShowSeason, PlexMediaSlim>();
        CreateProjection<PlexTvShowEpisode, PlexMediaSlim>();
        CreateProjection<PlexMovie, PlexMediaSlim>();
    }
}