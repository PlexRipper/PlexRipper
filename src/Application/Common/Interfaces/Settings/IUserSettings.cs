﻿using PlexRipper.Application.Settings.Models;
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
        bool Save();

        /// <summary>
        /// Reads the json settings file and updates all SettingsModel properties.
        /// </summary>
        /// <returns>Is successful.</returns>
        bool Load();

        /// <summary>
        /// Reverts all settings to their default value.
        /// </summary>
        void Reset();

        /// <summary>
        /// This will copy values from the sourceSettings and set them to this UserSettings
        /// The UserSettings also inherits from <see cref="SettingsModel"/> such that we can simply do "userSettings.ApiKey"
        /// instead of having a separate instance of the <see cref="SettingsModel"/> in the UserSettings.
        /// </summary>
        /// <param name="sourceSettings">The values to be used to set this UserSettings instance.</param>
        /// <param name="saveAfterUpdate">Should the settings be written to file after update.</param>
        void UpdateSettings(SettingsModel sourceSettings, bool saveAfterUpdate = true);
    }
}