using AutoMapper;
using PlexRipper.Domain.Entities;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.Common.DTO.PlexMedia;
using System.Linq;

namespace PlexRipper.WebAPI.Config
{
    public class WebApiMappingProfile : Profile
    {
        public WebApiMappingProfile()
        {
            //AccountDTO <-> Account
            CreateMap<AccountDTO, Account>(MemberList.Destination).ReverseMap();
            //PlexAccountDTO <-> PlexAccount
            CreateMap<PlexAccountDTO, PlexAccount>(MemberList.None)
                .ForMember(x => x.PlexAccountServers, opt => opt.Ignore());
            CreateMap<PlexAccount, PlexAccountDTO>(MemberList.Destination)
                .ForMember(dto => dto.PlexServers,
                    opt => opt.MapFrom(x => x.PlexAccountServers.ToArray().Select(y => y.PlexServer).ToList()));

            //PlexServer <-> PlexServerDTO
            CreateMap<PlexServerDTO, PlexServer>(MemberList.Source).ReverseMap();


            //PlexLibrary -> PlexLibraryDTO
            CreateMap<PlexLibrary, PlexLibraryDTO>(MemberList.Destination)
                .ForMember(dto => dto.Count, entity => entity.MapFrom(x => x.GetMediaCount)).ReverseMap();


            //PlexLibrary -> PlexLibraryDTO
            CreateMap<PlexLibrary, PlexLibraryContainerDTO>(MemberList.Destination)
                .ForMember(dto => dto.Count, entity => entity.MapFrom(x => x.GetMediaCount));

            //PlexLibrary -> PlexLibraryDTO
            CreateMap<PlexSerie, PlexSerieDTO>(MemberList.Destination);

            //PlexLibrary -> PlexLibraryDTO
            CreateMap<PlexMovie, PlexMovieDTO>(MemberList.Destination);

        }
    }
}
