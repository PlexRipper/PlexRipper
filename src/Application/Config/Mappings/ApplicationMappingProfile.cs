using AutoMapper;
using PlexApi.Contracts;

namespace PlexRipper.Application;

public class ApplicationMappingProfile : Profile
{
    public ApplicationMappingProfile()
    {
        // PlexApiClientProgress -> InspectServerProgress
        CreateMap<PlexApiClientProgress, InspectServerProgress>(MemberList.None);

        // PlexApiClientProgress -> ServerConnectionCheckStatusProgress
        CreateMap<PlexApiClientProgress, ServerConnectionCheckStatusProgress>(MemberList.None);
    }
}