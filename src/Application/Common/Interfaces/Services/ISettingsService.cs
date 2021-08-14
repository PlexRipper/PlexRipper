using FluentResults;

namespace PlexRipper.Application.Common
{
    public interface ISettingsService
    {
        Result<ISettingsModel> GetSettings();

        Result<ISettingsModel> UpdateSettings(ISettingsModel settingsModel);
    }
}