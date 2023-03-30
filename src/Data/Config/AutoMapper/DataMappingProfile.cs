using AutoMapper;

namespace PlexRipper.Data;

public class DataMappingProfile : Profile
{
    public DataMappingProfile()
    {
        CreateProjection<PlexMovie, Domain.PlexMedia>()
            .ForMember(x => x.SortTitle, opt => opt.MapFrom(x => x.SortTitle ?? x.Title));
        CreateProjection<PlexTvShow, Domain.PlexMedia>()
            .ForMember(x => x.SortTitle, opt => opt.MapFrom(x => x.SortTitle ?? x.Title));
        CreateProjection<PlexTvShowSeason, Domain.PlexMedia>()
            .ForMember(x => x.SortTitle, opt => opt.MapFrom(x => x.SortTitle ?? x.Title));
        CreateProjection<PlexTvShowEpisode, Domain.PlexMedia>()
            .ForMember(x => x.SortTitle, opt => opt.MapFrom(x => x.SortTitle ?? x.Title));

        CreateProjection<PlexMovie, PlexMediaSlim>()
            .ForMember(x => x.SortTitle, opt => opt.MapFrom(x => x.SortTitle ?? x.Title));
        CreateProjection<PlexTvShow, PlexMediaSlim>()
            .ForMember(x => x.SortTitle, opt => opt.MapFrom(x => x.SortTitle ?? x.Title));
        CreateProjection<PlexTvShowSeason, PlexMediaSlim>()
            .ForMember(x => x.SortTitle, opt => opt.MapFrom(x => x.SortTitle ?? x.Title));
        CreateProjection<PlexTvShowEpisode, PlexMediaSlim>()
            .ForMember(x => x.SortTitle, opt => opt.MapFrom(x => x.SortTitle ?? x.Title));
    }
}