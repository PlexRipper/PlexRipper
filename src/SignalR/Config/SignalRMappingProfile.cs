using AutoMapper;
using PlexRipper.Domain;
using PlexRipper.SignalR.Common;

namespace PlexRipper.SignalR.Config
{
    public class SignalRMappingProfile : Profile
    {
        public SignalRMappingProfile()
        {
            // Notification <-> NotificationUpdate
            CreateMap<Notification, NotificationDTO>(MemberList.Destination)
                .ForSourceMember(x => x.Level, opt => opt.DoNotValidate())
                .ForMember(x => x.Level, opt => opt.MapFrom(x => x.NotificationLevel)).ReverseMap();
        }
    }
}