using AutoMapper;
using PlexRipper.Domain.Entities.Plex;
using PlexRipper.Domain.ValueObjects;

namespace PlexRipper.Application.Common.Mappings
{
    public class InfrastructureProfile : Profile
    {
        public InfrastructureProfile()
        {
            CreateMap<PlexAccountDTO, PlexAccount>(MemberList.Destination).ReverseMap();
        }
    }
}
