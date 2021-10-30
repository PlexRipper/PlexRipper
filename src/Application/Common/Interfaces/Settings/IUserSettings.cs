using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common
{
    /// <summary>
    /// Used to store and load settings from a json file.
    /// </summary>
    public interface IUserSettings : ISettingsModel, ISetup
    {
        /// <summary>
        /// Writes all (nested) property values in the SettingsModel to the json settings file.
        /// </summary>
        /// <returns>Is successful.</returns>
        Result Save();

        /// <summary>
        /// Reads the json settings file and updates all SettingsModel properties.
        /// </summary>
        /// <returns>Is successful.</returns>
        Result Load();

        /// <summary>
        /// Reverts all settings to their default value.
        /// </summary>
        Result Reset();

        /// <summary>
        /// This will copy values from the sourceSettings and set them to this UserSettings
        /// The UserSettings also inherits from <see cref="ISettingsModel"/> such that we can simply do "userSettings.ApiKey"
        /// instead of having a separate instance of the <see cref="ISettingsModel"/> in the UserSettings.
        /// </summary>
        /// <param name="sourceSettings">The values to be used to set this UserSettings instance.</param>
        Result UpdateSettings(ISettingsModel sourceSettings);

        int GetDownloadSpeedLimit(string machineIdentifier);
    }
}