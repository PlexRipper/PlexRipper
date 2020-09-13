using PlexRipper.Domain.Settings;

namespace PlexRipper.Application.Common
{
    public interface ISettingsModel
    {
        AdvancedSettingsModel AdvancedSettings { get; }

        int ActiveAccountId { get; set; }
    }
}