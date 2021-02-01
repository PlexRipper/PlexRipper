using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Application.Common;
using PlexRipper.Application.Settings.Models;

namespace PlexRipper.Application.Settings
{
    /// <summary>
    /// Handles API requests concerning the <see cref="IUserSettings"/>.
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private readonly IUserSettings _userSettings;

        public SettingsService(IUserSettings userSettings)
        {
            _userSettings = userSettings;
        }

        public Result<SettingsModel> GetSettings()
        {
            return Result.Ok(_userSettings as SettingsModel);
        }

        public Result<SettingsModel> UpdateSettings(SettingsModel settingsModel)
        {
            _userSettings.UpdateSettings(settingsModel);
            return GetSettings();
        }
    }
}