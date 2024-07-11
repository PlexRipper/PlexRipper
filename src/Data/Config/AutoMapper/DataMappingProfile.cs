using AutoMapper;

namespace PlexRipper.Data;

public class DataMappingProfile : Profile
{
    public DataMappingProfile()
    {
        CreateProjection<PlexMovie, PlexMedia>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => PlexMediaType.Movie))
            .ForMember(x => x.SortTitle, opt => opt.MapFrom(x => x.SortTitle ?? x.Title));
        CreateProjection<PlexTvShow, PlexMedia>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => PlexMediaType.TvShow))
            .ForMember(x => x.SortTitle, opt => opt.MapFrom(x => x.SortTitle ?? x.Title));
        CreateProjection<PlexTvShowSeason, PlexMedia>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => PlexMediaType.Season))
            .ForMember(x => x.SortTitle, opt => opt.MapFrom(x => x.SortTitle ?? x.Title));
        CreateProjection<PlexTvShowEpisode, PlexMedia>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => PlexMediaType.Episode))
            .ForMember(x => x.SortTitle, opt => opt.MapFrom(x => x.SortTitle ?? x.Title));

        CreateProjection<PlexMovie, PlexMediaSlim>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => PlexMediaType.Movie))
            .ForMember(x => x.SortTitle, opt => opt.MapFrom(x => x.SortTitle ?? x.Title))
            .ForMember(x => x.Qualities, opt => opt.Ignore());
        CreateProjection<PlexTvShow, PlexMediaSlim>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => PlexMediaType.TvShow))
            .ForMember(x => x.SortTitle, opt => opt.MapFrom(x => x.SortTitle ?? x.Title))
            .ForMember(x => x.Qualities, opt => opt.Ignore());
        CreateProjection<PlexTvShowSeason, PlexMediaSlim>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => PlexMediaType.Season))
            .ForMember(x => x.SortTitle, opt => opt.MapFrom(x => x.SortTitle ?? x.Title))
            .ForMember(x => x.Qualities, opt => opt.Ignore());
        CreateProjection<PlexTvShowEpisode, PlexMediaSlim>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => PlexMediaType.Episode))
            .ForMember(x => x.SortTitle, opt => opt.MapFrom(x => x.SortTitle ?? x.Title))
            .ForMember(x => x.Qualities, opt => opt.Ignore());
    }
}
