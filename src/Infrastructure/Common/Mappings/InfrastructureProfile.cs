using AutoMapper;
using PlexRipper.Domain.Entities.Plex;
using PlexRipper.Infrastructure.Common.Models.Plex;

namespace PlexRipper.Infrastructure.Common.Mappings
{
    public class InfrastructureProfile : Profile
    {
        public InfrastructureProfile()
        {
            CreateMap<PlexAccountDTO, PlexAccount>(MemberList.Destination).ReverseMap();
        }
    }
}
