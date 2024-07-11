using Application.Contracts;
using AutoMapper;
using WebAPI.Contracts;

namespace PlexRipper.Application;

public class ApplicationMappingProfile : Profile
{
    public ApplicationMappingProfile()
    {
        // PlexApiClientProgress -> InspectServerProgress
        CreateMap<PlexApiClientProgress, InspectServerProgress>(MemberList.None);
    }
}
