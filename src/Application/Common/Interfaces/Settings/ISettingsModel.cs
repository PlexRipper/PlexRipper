using PlexRipper.Application.Settings.Models;

namespace PlexRipper.Application.Common
{
    public interface ISettingsModel
    {
        AccountSettingsModel AccountSettings { get; set; }

        AdvancedSettingsModel AdvancedSettings { get; set; }

        UserInterfaceSettingsModel UserInterfaceSettings { get; set; }
    }
}