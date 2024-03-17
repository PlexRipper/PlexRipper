using Application.Contracts;
using Application.Contracts.FolderPath;
using Application.Contracts.PlexMediaData;
using AutoMapper;
using BackgroundServices.Contracts;
using PlexRipper.WebAPI.Common;
using PlexRipper.WebAPI.SignalR.Common;
using WebAPI.Contracts;

namespace PlexRipper.WebAPI;

public class WebApiMappingProfile : Profile
{
    public WebApiMappingProfile()
    {
        // Result -> ResultDTO
        CreateMap<Result, ResultDTO>(MemberList.None);
        CreateMap<ResultDTO, Result>(MemberList.None)
            .ConvertUsing<ResultDTOToResult>();

        // .ConvertUsing<ResultToResultDTO>();
        CreateMap(typeof(Result<>), typeof(ResultDTO<>), MemberList.Destination).ReverseMap();
        CreateMap(typeof(Result<>), typeof(ResultDTO), MemberList.Destination).ReverseMap();
        CreateMap<IError, ErrorDTO>(MemberList.Destination).ReverseMap();
        CreateMap<ISuccess, SuccessDTO>(MemberList.Destination).ReverseMap();
        CreateMap<IReason, ReasonDTO>(MemberList.Destination).ReverseMap();

        // PlexLibrary -> PlexLibraryDTO
        CreateMap<PlexLibrary, PlexLibraryDTO>(MemberList.Destination)
            .ForMember(dto => dto.Count, entity => entity.MapFrom(x => x.MediaCount))
            .ForMember(dto => dto.SeasonCount, entity => entity.MapFrom(x => x.SeasonCount))
            .ForMember(dto => dto.EpisodeCount, entity => entity.MapFrom(x => x.EpisodeCount));

        // FolderPath -> FolderPathDTO
        CreateMap<FolderPath, FolderPathDTO>(MemberList.Destination)
            .ForMember(dto => dto.Directory, entity => entity.MapFrom(x => x.DirectoryPath))
            .ReverseMap();

        // FileSystemResult -> FileSystemDTO
        CreateMap<FileSystemResult, FileSystemDTO>(MemberList.Destination).ReverseMap();

        // FileSystemModel -> FileSystemModelDTO
        CreateMap<FileSystemModel, FileSystemModelDTO>(MemberList.Destination).ReverseMap();

        PlexAccountMappings();
        PlexServerMappings();
        PlexMediaMappings();
        PlexMovieMappings();
        PlexTvShowMappings();
        SignalRMappings();
    }

    private void PlexAccountMappings()
    {
        // CreatePlexAccountDTO -> PlexAccount
        CreateMap<UpdatePlexAccountDTO, PlexAccount>(MemberList.Source)
            .ReverseMap();

        // PlexAccountDTO <-> PlexAccount
        CreateMap<PlexAccountDTO, PlexAccount>(MemberList.None)
            .ForMember(x => x.PlexServers, opt => opt.Ignore())
            .ForMember(x => x.PlexAccountServers, opt => opt.Ignore());

        // PlexAccount -> PlexAccountDTO
        CreateMap<PlexAccount, PlexAccountDTO>(MemberList.Destination)
            .ForMember(dto => dto.PlexServerAccess, opt => opt.MapFrom(x => x.PlexAccountServers.Select(y => y.PlexServer).ToList()));

        CreateMap<PlexServer, PlexServerAccessDTO>(MemberList.Destination)
            .ForMember(dto => dto.PlexServerId, opt => opt.MapFrom(x => x.Id))
            .ForMember(dto => dto.PlexLibraryIds, opt => opt.MapFrom(x => x.PlexLibraries.Select(y => y.Id).ToList()));
    }

    private void PlexServerMappings()
    {
        // PlexServer -> PlexServerDTO
        CreateMap<PlexServer, PlexServerDTO>(MemberList.Destination);

        // PlexServerConnection -> PlexServerConnectionDTO
        CreateMap<PlexServerConnection, PlexServerConnectionDTO>(MemberList.Destination)
            .ForMember(dto => dto.Progress, opt => opt.Ignore())
            .ForMember(dto => dto.ServerStatusList, entity => entity.MapFrom(x => x.PlexServerStatus))
            .ForMember(dto => dto.LatestConnectionStatus, entity => entity.MapFrom(x => x.LatestConnectionStatus))
            .ForMember(dto => dto.Url, entity => entity.MapFrom(x => x.Url));

        // PlexServerStatus -> PlexServerStatusDTO
        CreateMap<PlexServerStatus, PlexServerStatusDTO>(MemberList.Destination);
    }

    private void PlexMediaMappings()
    {
        // PlexMediaSlim -> PlexMediaSlimDTO
        CreateMap<PlexMediaSlim, PlexMediaSlimDTO>(MemberList.Destination)
            .ForMember(dto => dto.Qualities, opt => opt.MapFrom(x => x.Qualities))
            .ForMember(dto => dto.ThumbUrl, opt => opt.MapFrom(x => x.FullThumbUrl))
            .ForMember(dto => dto.Children, opt => opt.Ignore())
            .ForMember(dto => dto.Index, opt => opt.Ignore());

        // PlexMediaData -> PlexMediaDataDTO
        CreateMap<PlexMedia, PlexMediaDTO>(MemberList.Destination)
            .IncludeBase<PlexMediaSlim, PlexMediaSlimDTO>()
            .ForMember(dto => dto.TvShowId, opt => opt.Ignore())
            .ForMember(dto => dto.TvShowSeasonId, opt => opt.Ignore())
            .ForMember(dto => dto.MediaData, opt => opt.Ignore())
            .ForMember(dto => dto.Children, opt => opt.Ignore())
            .ForMember(dto => dto.Index, opt => opt.Ignore());

        // PlexMediaData -> PlexMediaDataDTO
        CreateMap<PlexMediaData, PlexMediaDataDTO>(MemberList.Destination);

        // PlexMediaQuality -> PlexMediaQualityDTO
        CreateMap<PlexMediaQuality, PlexMediaQualityDTO>(MemberList.Destination);

        // PlexMediaDataPart -> PlexMediaDataPartDTO
        CreateMap<PlexMediaDataPart, PlexMediaDataPartDTO>(MemberList.Destination);
    }

    private void PlexMovieMappings()
    {
        // PlexMovie -> PlexMovieDTO
        CreateMap<PlexMovie, PlexMediaDTO>(MemberList.Destination)
            .IncludeBase<PlexMedia, PlexMediaDTO>()
            .ForMember(dto => dto.MediaData, entity => entity.MapFrom(x => x.MovieData));
    }

    private void PlexTvShowMappings()
    {
        // PlexTvShow -> PlexTvShowDTO
        CreateMap<PlexTvShow, PlexMediaDTO>(MemberList.Destination)
            .IncludeBase<PlexMedia, PlexMediaDTO>()
            .ForMember(dto => dto.TvShowId, opt => opt.MapFrom(entity => entity.Id))
            .ForMember(dto => dto.TvShowSeasonId, opt => opt.Ignore())
            .ForMember(dto => dto.Children, opt => opt.MapFrom(entity => entity.Seasons))
            .ForMember(dto => dto.MediaData, opt => opt.Ignore());

        // PlexTvShowSeason -> PlexTvShowSeasonDTO
        CreateMap<PlexTvShowSeason, PlexMediaDTO>(MemberList.Destination)
            .IncludeBase<PlexMedia, PlexMediaDTO>()
            .ForMember(dto => dto.TvShowSeasonId, opt => opt.MapFrom(entity => entity.Id))
            .ForMember(dto => dto.TvShowId, opt => opt.MapFrom(entity => entity.TvShowId))
            .ForMember(dto => dto.Children, opt => opt.MapFrom(entity => entity.Episodes))
            .ForMember(dto => dto.MediaData, opt => opt.Ignore());

        // PlexTvShowEpisode -> PlexTvShowEpisodeDTO
        CreateMap<PlexTvShowEpisode, PlexMediaDTO>(MemberList.Destination)
            .IncludeBase<PlexMedia, PlexMediaDTO>()
            .ForMember(dto => dto.Children, opt => opt.Ignore())
            .ForMember(dto => dto.TvShowId, opt => opt.MapFrom(entity => entity.TvShowId))
            .ForMember(dto => dto.TvShowSeasonId, opt => opt.MapFrom(entity => entity.TvShowSeasonId))
            .ForMember(dto => dto.MediaData, entity => entity.MapFrom(x => x.EpisodeData));

        // PlexTvShow -> PlexMediaSlimDTO
        CreateMap<PlexTvShow, PlexMediaSlimDTO>(MemberList.Destination)
            .IncludeBase<PlexMediaSlim, PlexMediaSlimDTO>()
            .ForMember(dto => dto.Children, opt => opt.MapFrom(entity => entity.Seasons))
            .ForMember(dto => dto.Index, opt => opt.Ignore());

        // PlexTvShowSeason -> PlexMediaSlimDTO
        CreateMap<PlexTvShowSeason, PlexMediaSlimDTO>(MemberList.Destination)
            .IncludeBase<PlexMediaSlim, PlexMediaSlimDTO>()
            .ForMember(dto => dto.Children, opt => opt.MapFrom(entity => entity.Episodes))
            .ForMember(dto => dto.Index, opt => opt.Ignore());

        // PlexTvShowEpisode -> PlexMediaSlimDTO
        CreateMap<PlexTvShowEpisode, PlexMediaSlimDTO>(MemberList.Destination)
            .IncludeBase<PlexMediaSlim, PlexMediaSlimDTO>()
            .ForMember(dto => dto.Children, opt => opt.Ignore())
            .ForMember(dto => dto.Index, opt => opt.Ignore());
    }

    private void SignalRMappings()
    {
        // InspectServerProgress -> InspectServerProgressDTO
        CreateMap<InspectServerProgress, InspectServerProgressDTO>(MemberList.Destination);

        // InspectServerProgress -> InspectServerProgressDTO
        CreateMap<ServerConnectionCheckStatusProgress, ServerConnectionCheckStatusProgressDTO>(MemberList.Destination);

        // Notification <-> NotificationUpdate
        CreateMap<Notification, NotificationDTO>(MemberList.Destination)
            .ReverseMap();

        // JobStatusUpdate -> JobStatusUpdateDTO
        CreateMap<JobStatusUpdate, JobStatusUpdateDTO>(MemberList.Destination)
            .ForMember(dto => dto.JobType, entity => entity.MapFrom(x => ToJobType(x.JobGroup)));
    }

    private static JobTypes ToJobType(string jobGroup) => Enum.TryParse<JobTypes>(jobGroup, out var jobType) ? jobType : JobTypes.Unknown;
}