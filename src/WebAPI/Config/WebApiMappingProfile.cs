using AutoMapper;
using PlexRipper.Domain.Entities;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.Common.DTO.PlexMedia;

namespace PlexRipper.WebAPI.Config
{
    public class WebApiMappingProfile : Profile
    {
        public WebApiMappingProfile()
        {
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
            CreateMap<PlexServerDTO, PlexServer>(MemberList.Source).ReverseMap();


            //PlexLibrary -> PlexLibraryDTO
            CreateMap<PlexLibrary, PlexLibraryDTO>(MemberList.Destination)
                .ForMember(dto => dto.Count, entity => entity.MapFrom(x => x.GetMediaCount)).ReverseMap();


            //PlexLibrary -> PlexLibraryDTO
            CreateMap<PlexLibrary, PlexLibraryContainerDTO>(MemberList.Destination)
                .ForMember(dto => dto.Count, entity => entity.MapFrom(x => x.GetMediaCount));

            //PlexSerie -> PlexSerieDTO
            CreateMap<PlexTvShow, PlexTvShowDTO>(MemberList.Destination).ReverseMap();

            //PlexMovie -> PlexMovieDTO
            CreateMap<PlexMovie, PlexMovieDTO>(MemberList.Destination).ReverseMap();

            //DownloadTask -> DownloadTaskDTO
            CreateMap<DownloadTask, DownloadTaskDTO>(MemberList.Destination)
                .ForMember(dto => dto.Status, entity => entity.MapFrom(x => x.DownloadStatus));

        }
    }
}
