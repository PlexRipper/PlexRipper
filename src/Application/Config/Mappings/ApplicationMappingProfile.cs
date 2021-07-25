using AutoMapper;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.WebApi;

namespace PlexRipper.Application.Config
{
    public class ApplicationMappingProfile : Profile
    {
        public ApplicationMappingProfile()
        {
            // PlexApiClientProgress -> InspectServerProgress
            CreateMap<PlexApiClientProgress, InspectServerProgress>(MemberList.None)
                .ReverseMap();
        }
    }
}