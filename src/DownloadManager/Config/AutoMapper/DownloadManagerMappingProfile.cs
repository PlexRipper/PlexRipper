using System;
using System.Collections.Generic;
using AutoMapper;
using PlexRipper.Domain;

namespace PlexRipper.DownloadManager.AutoMapper
{
    public class DownloadManagerMappingProfile : Profile
    {
        public DownloadManagerMappingProfile()
        {
            PlexMediaToDownloadTask();
        }

        private void PlexMediaToDownloadTask()
        {
            CreateMap<PlexMedia, DownloadTask>(MemberList.None)
                .ForMember(task => task.Key, opt => opt.MapFrom(entity => entity.Key))
                .ForMember(task => task.Title, opt => opt.MapFrom(entity => entity.Title))
                .ForMember(task => task.FullTitle, opt => opt.MapFrom(entity => entity.FullTitle))
                .ForMember(task => task.Year, opt => opt.MapFrom(entity => entity.Year))
                .ForMember(task => task.MediaType, opt => opt.MapFrom(entity => entity.Type))
                .ForMember(task => task.DownloadStatus, opt => opt.MapFrom(entity => DownloadStatus.Initialized))
                .ForMember(task => task.Children, opt => opt.MapFrom(entity => new List<DownloadTask>()))
                .ForMember(task => task.Created, opt => opt.MapFrom(entity => DateTime.UtcNow))
                .ForMember(task => task.PlexLibrary, opt => opt.MapFrom(entity => entity.PlexLibrary))
                .ForMember(task => task.PlexLibraryId, opt => opt.MapFrom(entity => entity.PlexLibraryId))
                .ForMember(task => task.PlexServer, opt => opt.MapFrom(entity => entity.PlexServer))
                .ForMember(task => task.PlexServerId, opt => opt.MapFrom(entity => entity.PlexServerId));

            CreateMap<PlexMovie, DownloadTask>(MemberList.None)
                .IncludeBase<PlexMedia, DownloadTask>()
                .ForMember(task => task.DownloadTaskType, opt => opt.MapFrom(entity => DownloadTaskType.Movie));

            CreateMap<PlexTvShow, DownloadTask>(MemberList.None)
                .IncludeBase<PlexMedia, DownloadTask>()
                .ForMember(task => task.DownloadTaskType, opt => opt.MapFrom(entity => DownloadTaskType.TvShow));

            CreateMap<PlexTvShowSeason, DownloadTask>(MemberList.None)
                .IncludeBase<PlexMedia, DownloadTask>()
                .ForMember(task => task.DownloadTaskType, opt => opt.MapFrom(entity => DownloadTaskType.Season));

            CreateMap<PlexTvShowEpisode, DownloadTask>(MemberList.None)
                .IncludeBase<PlexMedia, DownloadTask>()
                .ForMember(task => task.DownloadTaskType, opt => opt.MapFrom(entity => DownloadTaskType.Episode));

            CreateMap<PlexMediaData, DownloadTask>(MemberList.None);
        }
    }
}