using AutoMapper;
using PlexRipper.Settings.Models;
using Settings.Contracts;

namespace PlexRipper.Settings.Config;

public class SettingsMappingProfile : Profile
{
    public SettingsMappingProfile()
    {
        DtoToInterface();
        InterfaceToDto();
    }

    private void DtoToInterface()
    {
        CreateMap<SettingsModelDTO, SettingsModel>();
        CreateMap<GeneralSettingsDTO, GeneralSettings>();
        CreateMap<ConfirmationSettingsDTO, ConfirmationSettings>();
        CreateMap<DateTimeSettingsDTO, DateTimeSettings>();
        CreateMap<DisplaySettingsDTO, DisplaySettings>();
        CreateMap<LanguageSettingsDTO, LanguageSettings>();
        CreateMap<DownloadManagerSettingsDTO, DownloadManagerSettings>();
        CreateMap<ServerSettingsDTO, ServerSettings>();
        CreateMap<DebugSettingsDTO, DebugSettings>();

        CreateMap<SettingsModelDTO, ISettingsModel>().As<SettingsModel>();
        CreateMap<GeneralSettingsDTO, IGeneralSettings>().As<GeneralSettings>();
        CreateMap<ConfirmationSettingsDTO, IConfirmationSettings>().As<ConfirmationSettings>();
        CreateMap<DateTimeSettingsDTO, IDateTimeSettings>().As<DateTimeSettings>();
        CreateMap<DisplaySettingsDTO, IDisplaySettings>().As<DisplaySettings>();
        CreateMap<LanguageSettingsDTO, ILanguageSettings>().As<LanguageSettings>();
        CreateMap<DownloadManagerSettingsDTO, IDownloadManagerSettings>().As<DownloadManagerSettings>();
        CreateMap<ServerSettingsDTO, IServerSettings>().As<ServerSettings>();
        CreateMap<DebugSettingsDTO, IDebugSettings>().As<DebugSettings>();
    }

    private void InterfaceToDto()
    {
        CreateMap<ISettingsModel, SettingsModelDTO>();
        CreateMap<IGeneralSettings, GeneralSettingsDTO>();
        CreateMap<IConfirmationSettings, ConfirmationSettingsDTO>();
        CreateMap<IDateTimeSettings, DateTimeSettingsDTO>();
        CreateMap<IDisplaySettings, DisplaySettingsDTO>();
        CreateMap<IDownloadManagerSettings, DownloadManagerSettingsDTO>();
        CreateMap<ILanguageSettings, LanguageSettingsDTO>();
        CreateMap<IServerSettings, ServerSettingsDTO>();
        CreateMap<IDebugSettings, DebugSettingsDTO>();
    }
}