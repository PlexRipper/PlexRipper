using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FluentResults;
using PlexRipper.Application;
using PlexRipper.Domain;
using PlexRipper.DownloadManager;
using PlexRipper.Settings.Models;
using PlexRipper.Settings.Modules;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.Common.DTO.FolderPath;
using PlexRipper.WebAPI.Common.DTO.PlexMediaData;
using PlexRipper.WebAPI.Common.FluentResult;
using PlexRipper.WebAPI.SignalR.Common;

namespace PlexRipper.WebAPI.Config
{
    public class WebApiMappingProfile : Profile
    {
        public WebApiMappingProfile()
        {
            // Result -> ResultDTO
            CreateMap<Result, ResultDTO>(MemberList.None);
            CreateMap(typeof(Result<>), typeof(ResultDTO<>), MemberList.None);
            CreateMap(typeof(Result<>), typeof(ResultDTO), MemberList.None);

            // PlexServer -> PlexServerDTO
            CreateMap<PlexServer, PlexServerDTO>(MemberList.Destination)
                .ForMember(dto => dto.Status, entity => entity.MapFrom(x => x.Status));

            // PlexServerStatus -> PlexServerStatusDTO
            CreateMap<PlexServerStatus, PlexServerStatusDTO>(MemberList.Destination);

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

            // Notification <-> NotificationUpdate
            CreateMap<Notification, NotificationDTO>(MemberList.Destination)
                .ReverseMap();

            PlexAccountMappings();
            DownloadTaskMappings();
            PlexMediaMappings();
            PlexMovieMappings();
            PlexTvShowMappings();
            SettingsMappings();
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
                .ForMember(dto => dto.PlexServers, opt => opt.MapFrom(x => x.PlexAccountServers.Select(y => y.PlexServer).ToList()));
        }

        private void DownloadTaskMappings()
        {
            CreateMap<DownloadTask, DownloadTaskDTO>(MemberList.Destination)
                .ForMember(dto => dto.Status, opt => opt.MapFrom(entity => entity.DownloadStatus))
                .ForMember(dto => dto.Actions, opt => opt.MapFrom(entity => DownloadTaskActions.Convert(entity.DownloadStatus)));

            CreateMap<DownloadTask, DownloadProgressDTO>(MemberList.Destination)
                .ForMember(dto => dto.Status, opt => opt.MapFrom(entity => entity.DownloadStatus))
                .ForMember(dto => dto.Actions, opt => opt.MapFrom(entity => DownloadTaskActions.Convert(entity.DownloadStatus)));

            CreateMap<List<DownloadTask>, List<ServerDownloadProgressDTO>>(MemberList.None)
                .ConstructUsing((list, context) =>
                {
                    var serverDownloads = new List<ServerDownloadProgressDTO>();
                    foreach (var serverId in list.Select(x => x.PlexServerId).Distinct())
                    {
                        var downloadTasks = list.Where(x => x.PlexServerId == serverId).ToList();
                        var downloadTaskDTO = context.Mapper.Map<List<DownloadProgressDTO>>(downloadTasks);
                        serverDownloads.Add(new ServerDownloadProgressDTO
                        {
                            Id = serverId,
                            Downloads = downloadTaskDTO,
                        });
                    }

                    return serverDownloads;
                });
        }

        private void PlexMediaMappings()
        {
            // PlexMediaData -> PlexMediaDataDTO
            CreateMap<PlexMedia, PlexMediaDTO>(MemberList.Destination)
                .ForMember(dto => dto.TreeKeyId, opt => opt.MapFrom(entity => entity.Id.ToString()))
                .ForMember(dto => dto.TvShowId, opt => opt.Ignore())
                .ForMember(dto => dto.TvShowSeasonId, opt => opt.Ignore())
                .ForMember(dto => dto.MediaData, opt => opt.Ignore())
                .ForMember(dto => dto.Children, opt => opt.Ignore());

            // PlexMediaData -> PlexMediaDataDTO
            CreateMap<PlexMediaData, PlexMediaDataDTO>(MemberList.Destination);

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
                .ForMember(dto => dto.TreeKeyId, opt => opt.MapFrom(entity => entity.Id.ToString()))
                .ForMember(dto => dto.TvShowSeasonId, opt => opt.Ignore())
                .ForMember(dto => dto.Children, opt => opt.MapFrom(entity => entity.Seasons))
                .ForMember(dto => dto.MediaData, opt => opt.Ignore());

            // PlexTvShowSeason -> PlexTvShowSeasonDTO
            CreateMap<PlexTvShowSeason, PlexMediaDTO>(MemberList.Destination)
                .IncludeBase<PlexMedia, PlexMediaDTO>()
                .ForMember(dto => dto.TvShowSeasonId, opt => opt.MapFrom(entity => entity.Id))
                .ForMember(dto => dto.TreeKeyId, opt => opt.MapFrom(entity => $"{entity.TvShowId.ToString()}-{entity.Id.ToString()}"))
                .ForMember(dto => dto.TvShowId, opt => opt.MapFrom(entity => entity.TvShowId))
                .ForMember(dto => dto.Children, opt => opt.MapFrom(entity => entity.Episodes))
                .ForMember(dto => dto.MediaData, opt => opt.Ignore());

            // PlexTvShowEpisode -> PlexTvShowEpisodeDTO
            CreateMap<PlexTvShowEpisode, PlexMediaDTO>(MemberList.Destination)
                .IncludeBase<PlexMedia, PlexMediaDTO>()
                .ForMember(dto => dto.Children, opt => opt.Ignore())
                .ForMember(dto => dto.TreeKeyId,
                    opt => opt.MapFrom(entity => $"{entity.TvShowId.ToString()}-{entity.TvShowSeasonId.ToString()}-{entity.Id.ToString()}"))
                .ForMember(dto => dto.TvShowId, opt => opt.MapFrom(entity => entity.TvShowId))
                .ForMember(dto => dto.TvShowSeasonId, opt => opt.MapFrom(entity => entity.TvShowSeasonId))
                .ForMember(dto => dto.MediaData, entity => entity.MapFrom(x => x.EpisodeData));
        }

        private void SettingsMappings()
        {
            CreateMap<ISettingsModel, SettingsModelDTO>(MemberList.Destination)
                .ForMember(dto => dto.GeneralSettings, entity => entity.MapFrom(x => x.GeneralSettings));

            CreateMap<SettingsModelDTO, SettingsModel>();
            CreateMap<GeneralSettingsDTO, GeneralSettingsModule>().ReverseMap();
            CreateMap<GeneralSettingsDTO, IGeneralSettingsModule>().ReverseMap();
        }
    }
}