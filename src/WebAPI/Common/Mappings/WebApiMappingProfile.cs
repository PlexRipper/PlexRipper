using AutoMapper;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Entities.Plex;
using PlexRipper.Infrastructure.Common.DTO;
using PlexRipper.WebAPI.Common.DTO;
using System.Linq;
using PlexRipper.Infrastructure.Common.DTO.PlexGetServer;
using PlexAccountDTO = PlexRipper.WebAPI.Common.DTO.PlexAccountDTO;

namespace PlexRipper.WebAPI.Common.Mappings
{
    public class WebApiMappingProfile : Profile
    {
        public WebApiMappingProfile()
        {
            //AccountDTO <-> Account
            CreateMap<AccountDTO, Account>(MemberList.Destination).ReverseMap();
            //PlexAccountDTO <-> PlexAccount
            CreateMap<PlexAccountDTO, PlexAccount>(MemberList.None)
                .ForMember(x => x.PlexAccountServers,
                    opt => opt.Ignore());
            CreateMap<PlexAccount, PlexAccountDTO>(MemberList.Destination)
                .ForMember(dto => dto.PlexServers,
                    opt => opt.MapFrom(x => x.PlexAccountServers.ToArray().Select(y => y.PlexServer).ToList()));


            CreateMap<PlexServerDTO, PlexServer>(MemberList.Destination)
                .ReverseMap();
        }
    }
}
