﻿using System.Linq;
using AutoMapper;
using FluentResults;
using PlexRipper.Domain;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.Common.DTO.FolderPath;
using PlexRipper.WebAPI.Common.DTO.PlexMediaData;
using PlexRipper.WebAPI.Common.FluentResult;

namespace PlexRipper.WebAPI.Config
{
    public class WebApiMappingProfile : Profile
    {
        public WebApiMappingProfile()
        {
            // Result -> ResultDTO
            CreateMap<Result, ResultDTO>(MemberList.None);
            CreateMap(typeof(Result<>), typeof(ResultDTO<>), MemberList.None);

            // CreatePlexAccountDTO -> PlexAccount
            CreateMap<CreatePlexAccountDTO, PlexAccount>(MemberList.Source)
                .ReverseMap();

            // CreatePlexAccountDTO -> PlexAccount
            CreateMap<UpdatePlexAccountDTO, PlexAccount>(MemberList.Source)
                .ReverseMap();

            // PlexAccountDTO <-> PlexAccount
            CreateMap<PlexAccountDTO, PlexAccount>(MemberList.None)
                .ForMember(x => x.PlexServers, opt => opt.Ignore())
                .ForMember(x => x.PlexAccountServers, opt => opt.Ignore());

            CreateMap<PlexAccount, PlexAccountDTO>(MemberList.Destination)
                .ForMember(dto => dto.AuthToken, opt => opt.MapFrom(x => x.AuthenticationToken))
                .ForMember(dto => dto.PlexServers, opt => opt.MapFrom(x => x.PlexAccountServers.Select(y => y.PlexServer).ToList()));

            // PlexServer -> PlexServerDTO
            CreateMap<PlexServer, PlexServerDTO>(MemberList.Destination)
                .ForMember(dto => dto.Status, entity => entity.MapFrom(x => x.Status));

            // PlexServerStatus -> PlexServerStatusDTO
            CreateMap<PlexServerStatus, PlexServerStatusDTO>(MemberList.Destination);

            // PlexLibrary -> PlexLibraryDTO
            CreateMap<PlexLibrary, PlexLibraryDTO>(MemberList.Destination)
                .ForMember(dto => dto.Count, entity => entity.MapFrom(x => x.MediaCount))
                .ForMember(dto => dto.MediaSize, entity => entity.MapFrom(x => x.MediaSize))
                .ForMember(dto => dto.SeasonCount, entity => entity.MapFrom(x => x.SeasonCount))
                .ForMember(dto => dto.EpisodeCount, entity => entity.MapFrom(x => x.EpisodeCount));

            // PlexTvShow <-> PlexTvShowDTO
            CreateMap<PlexTvShow, PlexTvShowDTO>(MemberList.Destination)
                .ForMember(dto => dto.Size, entity => entity.MapFrom(x => x.MediaSize))
                .ReverseMap();

            // PlexTvShowSeason <-> PlexTvShowSeasonDTO
            CreateMap<PlexTvShowSeason, PlexTvShowSeasonDTO>(MemberList.Destination)
                .ForMember(dto => dto.Size, entity => entity.MapFrom(x => x.MediaSize))
                .ReverseMap();

            // PlexTvShowEpisode <-> PlexTvShowEpisodeDTO
            CreateMap<PlexTvShowEpisode, PlexTvShowEpisodeDTO>(MemberList.Destination)
                .ForMember(dto => dto.Size, entity => entity.MapFrom(x => x.MediaSize))
                .ReverseMap();

            // PlexMovie -> PlexMovieDTO
            CreateMap<PlexMovie, PlexMovieDTO>(MemberList.Destination)
                .ForMember(dto => dto.Size, entity => entity.MapFrom(x => x.GetParts.SelectMany(y => y.PlexMovieData.Parts).Sum(z => z.Size)));

            // PlexMovieData -> PlexMovieDataDTO
            CreateMap<PlexMovieData, PlexMovieDataDTO>(MemberList.Destination);

            // PlexMovieDataPart -> PlexMovieDataPartDTO
            CreateMap<PlexMovieDataPart, PlexMovieDataPartDTO>(MemberList.Destination);

            // DownloadTask -> DownloadTaskDTO
            CreateMap<DownloadTask, DownloadTaskDTO>(MemberList.Destination)
                .ForMember(dto => dto.Status, entity => entity.MapFrom(x => x.DownloadStatus));

            // FolderPath -> FolderPathDTO
            CreateMap<FolderPath, FolderPathDTO>(MemberList.Destination)
                .ForMember(dto => dto.Directory, entity => entity.MapFrom(x => x.DirectoryPath))
                .ReverseMap();

            // FileSystemResult -> FileSystemDTO
            CreateMap<FileSystemResult, FileSystemDTO>(MemberList.Destination).ReverseMap();

            // FileSystemModel -> FileSystemModelDTO
            CreateMap<FileSystemModel, FileSystemModelDTO>(MemberList.Destination).ReverseMap();
        }
    }
}