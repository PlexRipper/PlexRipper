using System;
using System.Text.Json;
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    /// <summary>
    /// Used to store and load settings from a json file.
    /// </summary>
    public interface IUserSettings : ISettingsModel
    {
        /// <summary>
        /// Reverts all settings to their default value.
        /// </summary>
        void Reset();

        /// <summary>
        /// This will copy values from the sourceSettings and set them to this UserSettings
        /// The UserSettings also inherits from <see cref="ISettingsModel"/> such that we can simply do "userSettings.ApiKey"
        /// instead of having a separate instance of the <see cref="ISettingsModel"/> in the UserSettings.
        /// </summary>
        /// <param name="sourceSettings">The values to be used to set this UserSettings instance.</param>
        Result<ISettingsModel> UpdateSettings(ISettingsModel sourceSettings);

        int GetDownloadSpeedLimit(string machineIdentifier);

        Result<string> GetJsonSettingsObject();

        IObservable<ISettingsModel> SettingsUpdated { get; }

        /// <summary>
        /// Parses the Json Element from the PlexRipperSettings.json and defaults its value if nothing is found.
        /// This also works when adding new settings and ensuring old config files get used as much as possible.
        /// </summary>
        /// <param name="settingsJsonElement"></param>
        Result SetFromJsonObject(JsonElement settingsJsonElement);
    }
}