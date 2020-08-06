using AutoMapper;
using FluentResults;
using PlexRipper.Domain.Entities;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.Common.FluentResult;

namespace PlexRipper.WebAPI.Config
{
    public class WebApiMappingProfile : Profile
    {
        public WebApiMappingProfile()
        {
            //Result -> ResultDTO
            CreateMap<Result, ResultDTO>(MemberList.Destination)
                .ForMember(x => x.Value, opt => opt.Ignore());

            //CreatePlexAccountDTO -> PlexAccount
            CreateMap<CreatePlexAccountDTO, PlexAccount>(MemberList.Source)
                .ReverseMap();

            //CreatePlexAccountDTO -> PlexAccount
            CreateMap<UpdatePlexAccountDTO, PlexAccount>(MemberList.Source)
                .ReverseMap();

            //PlexAccountDTO <-> PlexAccount
            CreateMap<PlexAccountDTO, PlexAccount>(MemberList.None)
                .ForMember(x => x.PlexAccountServers, opt => opt.Ignore());
            CreateMap<PlexAccount, PlexAccountDTO>(MemberList.Destination)
                .ForMember(dto => dto.AuthToken, opt => opt.MapFrom(x => x.AuthenticationToken))
                .ForMember(dto => dto.PlexServers, opt => opt.MapFrom(x => x.PlexServers));

            //PlexServer <-> PlexServerDTO
            CreateMap<PlexServer, PlexServerDTO>(MemberList.Destination).ReverseMap();


            //PlexLibrary <-> PlexLibraryDTO
            CreateMap<PlexLibrary, PlexLibraryDTO>(MemberList.Destination)
                .ForMember(dto => dto.Count, entity => entity.MapFrom(x => x.GetMediaCount)).ReverseMap();


            //PlexLibrary -> PlexLibraryDTO
            CreateMap<PlexLibrary, PlexLibraryContainerDTO>(MemberList.Destination)
                .ForMember(dto => dto.Count, entity => entity.MapFrom(x => x.GetMediaCount));

            //PlexTvShow <-> PlexTvShowDTO
            CreateMap<PlexTvShow, PlexTvShowDTO>(MemberList.Destination).ReverseMap();

            //PlexTvShowSeason <-> PlexTvShowSeasonDTO
            CreateMap<PlexTvShowSeason, PlexTvShowSeasonDTO>(MemberList.Destination).ReverseMap();

            //PlexTvShowSeason <-> PlexTvShowSeasonDTO
            CreateMap<PlexTvShowEpisode, PlexTvShowEpisodeDTO>(MemberList.Destination).ReverseMap();

            //PlexMovie -> PlexMovieDTO
            CreateMap<PlexMovie, PlexMovieDTO>(MemberList.Destination).ReverseMap();

            //DownloadTask -> DownloadTaskDTO
            CreateMap<DownloadTask, DownloadTaskDTO>(MemberList.Destination)
                .ForMember(dto => dto.Status, entity => entity.MapFrom(x => x.DownloadStatus));

        }
    }
}
