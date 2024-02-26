using AutoMapper;
using PlexApi.Contracts;
using WebAPI.Contracts;

namespace PlexRipper.Application;

public class ApplicationMappingProfile : Profile
{
    public ApplicationMappingProfile()
    {
        // PlexApiClientProgress -> InspectServerProgress
        CreateMap<PlexApiClientProgress, InspectServerProgress>(MemberList.None);

        // PlexApiClientProgress -> ServerConnectionCheckStatusProgress
        CreateMap<PlexApiClientProgress, ServerConnectionCheckStatusProgress>(MemberList.None);

        DownloadPreviewProjection();
    }

    private void DownloadPreviewProjection()
    {
        CreateProjection<PlexMovie, DownloadPreview>()
            .ForMember(x => x.Children, opt => opt.Ignore())
            .ForMember(x => x.TvShowId, opt => opt.Ignore())
            .ForMember(x => x.SeasonId, opt => opt.Ignore())
            .ForMember(x => x.Size, opt => opt.MapFrom(x => x.MediaSize))
            .ForMember(x => x.MediaType, opt => opt.MapFrom(x => PlexMediaType.Movie));
        CreateProjection<PlexTvShow, DownloadPreview>()
            .ForMember(x => x.Children, opt => opt.Ignore())
            .ForMember(x => x.TvShowId, opt => opt.Ignore())
            .ForMember(x => x.SeasonId, opt => opt.Ignore())
            .ForMember(x => x.Size, opt => opt.MapFrom(x => x.MediaSize))
            .ForMember(x => x.MediaType, opt => opt.MapFrom(x => PlexMediaType.TvShow));
        CreateProjection<PlexTvShowSeason, DownloadPreview>()
            .ForMember(x => x.Children, opt => opt.Ignore())
            .ForMember(x => x.SeasonId, opt => opt.Ignore())
            .ForMember(x => x.TvShowId, opt => opt.MapFrom(x => x.TvShowId))
            .ForMember(x => x.Size, opt => opt.MapFrom(x => x.MediaSize))
            .ForMember(x => x.MediaType, opt => opt.MapFrom(x => PlexMediaType.Season));
        CreateProjection<PlexTvShowEpisode, DownloadPreview>()
            .ForMember(x => x.Children, opt => opt.Ignore())
            .ForMember(x => x.TvShowId, opt => opt.MapFrom(x => x.TvShowId))
            .ForMember(x => x.SeasonId, opt => opt.MapFrom(x => x.TvShowSeasonId))
            .ForMember(x => x.Size, opt => opt.MapFrom(x => x.MediaSize))
            .ForMember(x => x.MediaType, opt => opt.MapFrom(x => PlexMediaType.Episode));

        CreateProjection<PlexTvShowEpisode, TvShowEpisodeKeyDTO>()
            .ForMember(x => x.TvShowId, opt => opt.MapFrom(x => x.TvShowId))
            .ForMember(x => x.SeasonId, opt => opt.MapFrom(x => x.TvShowSeasonId))
            .ForMember(x => x.EpisodeId, opt => opt.MapFrom(x => x.Id));
    }
}