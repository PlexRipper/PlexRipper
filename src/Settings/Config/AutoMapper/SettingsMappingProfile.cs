using AutoMapper;
using PlexRipper.Application;

namespace PlexRipper.Settings.Config
{
    public class SettingsMappingProfile : Profile
    {
        public SettingsMappingProfile()
        {
            // IUserSettings -> ISettingsModel
            CreateMap<IUserSettings, ISettingsModel>(MemberList.Destination);
        }
    }
}