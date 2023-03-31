using AutoMapper;
using Data.Contracts;
using DownloadManager.Contracts;

namespace PlexRipper.Data;

public class DataMappingProfile : Profile
{
    public DataMappingProfile()
    {
        CreateProjection<PlexMovie, Domain.PlexMedia>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => PlexMediaType.Movie))
            .ForMember(x => x.SortTitle, opt => opt.MapFrom(x => x.SortTitle ?? x.Title));
        CreateProjection<PlexTvShow, Domain.PlexMedia>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => PlexMediaType.TvShow))
            .ForMember(x => x.SortTitle, opt => opt.MapFrom(x => x.SortTitle ?? x.Title));
        CreateProjection<PlexTvShowSeason, Domain.PlexMedia>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => PlexMediaType.Season))
            .ForMember(x => x.SortTitle, opt => opt.MapFrom(x => x.SortTitle ?? x.Title));
        CreateProjection<PlexTvShowEpisode, Domain.PlexMedia>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => PlexMediaType.Episode))
            .ForMember(x => x.SortTitle, opt => opt.MapFrom(x => x.SortTitle ?? x.Title));

        CreateProjection<PlexMovie, PlexMediaSlim>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => PlexMediaType.Movie))
            .ForMember(x => x.SortTitle, opt => opt.MapFrom(x => x.SortTitle ?? x.Title));
        CreateProjection<PlexTvShow, PlexMediaSlim>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => PlexMediaType.TvShow))
            .ForMember(x => x.SortTitle, opt => opt.MapFrom(x => x.SortTitle ?? x.Title));
        CreateProjection<PlexTvShowSeason, PlexMediaSlim>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => PlexMediaType.Season))
            .ForMember(x => x.SortTitle, opt => opt.MapFrom(x => x.SortTitle ?? x.Title));
        CreateProjection<PlexTvShowEpisode, PlexMediaSlim>()
            .ForMember(x => x.Type, opt => opt.MapFrom(x => PlexMediaType.Episode))
            .ForMember(x => x.SortTitle, opt => opt.MapFrom(x => x.SortTitle ?? x.Title));

        CreateProjection<PlexMovie, DownloadPreviewDTO>()
            .ForMember(x => x.Children, opt => opt.Ignore())
            .ForMember(x => x.TvShowId, opt => opt.Ignore())
            .ForMember(x => x.SeasonId, opt => opt.Ignore())
            .ForMember(x => x.Type, opt => opt.MapFrom(x => PlexMediaType.Movie));
        CreateProjection<PlexTvShow, DownloadPreviewDTO>()
            .ForMember(x => x.Children, opt => opt.Ignore())
            .ForMember(x => x.TvShowId, opt => opt.Ignore())
            .ForMember(x => x.SeasonId, opt => opt.Ignore())
            .ForMember(x => x.Type, opt => opt.MapFrom(x => PlexMediaType.TvShow));
        CreateProjection<PlexTvShowSeason, DownloadPreviewDTO>()
            .ForMember(x => x.Children, opt => opt.Ignore())
            .ForMember(x => x.SeasonId, opt => opt.Ignore())
            .ForMember(x => x.TvShowId, opt => opt.MapFrom(x => x.TvShowId))
            .ForMember(x => x.Type, opt => opt.MapFrom(x => PlexMediaType.Season));
        CreateProjection<PlexTvShowEpisode, DownloadPreviewDTO>()
            .ForMember(x => x.Children, opt => opt.Ignore())
            .ForMember(x => x.TvShowId, opt => opt.MapFrom(x => x.TvShowId))
            .ForMember(x => x.SeasonId, opt => opt.MapFrom(x => x.TvShowSeasonId))
            .ForMember(x => x.Type, opt => opt.MapFrom(x => PlexMediaType.Episode));

        CreateProjection<PlexTvShowEpisode, TvShowEpisodeKeyDTO>()
            .ForMember(x => x.TvShowId, opt => opt.MapFrom(x => x.TvShowId))
            .ForMember(x => x.SeasonId, opt => opt.MapFrom(x => x.TvShowSeasonId))
            .ForMember(x => x.EpisodeId, opt => opt.MapFrom(x => x.Id));
    }
}