using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentResults;
using PlexRipper.Application.Common;
using PlexRipper.Domain;
using PlexRipper.Settings.Models;

namespace PlexRipper.Settings
{
    /// <inheritdoc cref="IUserSettings"/>
    public class UserSettings : SettingsModel, IUserSettings
    {
        #region Fields

        private readonly JsonSerializerOptions _jsonSerializerSettings = new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = false,
            PropertyNameCaseInsensitive = true,
        };

        private bool _allowSave = true;

        #endregion

        #region Properties

        [JsonIgnore]
        private static string FileName => "PlexRipperSettings.json";

        [JsonIgnore]
        private string FileLocation => Path.Join(FileSystemPaths.ConfigDirectory, FileName);

        #endregion

        #region Methods

        #region Public

        public Result Setup()
        {
            Log.Information("Setting up UserSettings");
            if (!File.Exists(FileLocation))
            {
                Log.Information($"{FileName} doesn't exist, will create new one now in {FileLocation}");
                var saveResult = Save();
                if (saveResult.IsFailed)
                {
                    return saveResult;
                }
            }

            return Load();
        }

        public Result Load()
        {
            Log.Information("Loading UserSettings now.");

            try
            {
                string jsonString = File.ReadAllText(FileLocation);
                var loadedSettings = JsonSerializer.Deserialize<dynamic>(jsonString, _jsonSerializerSettings);
                SetFromJsonObject(loadedSettings);
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError("Failed to load the UserSettings to json file.", e)).LogError();
            }

            return Result.Ok().WithSuccess("UserSettings were loaded successfully!").LogInformation();
        }

        public void Reset()
        {
            Log.Debug("Resetting UserSettings");

            UpdateSettings(new SettingsModel());
            Save();
        }

        /// <inheritdoc/>
        public Result Save()
        {
            if (!_allowSave)
            {
                return Result.Fail("UserSettings is denied from saving by the allowSave lock").LogWarning();
            }

            Log.Information("Saving UserSettings now.");

            try
            {
                string jsonString = JsonSerializer.Serialize(GetJsonObject(), _jsonSerializerSettings);
                File.WriteAllText(FileLocation, jsonString);
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError("Failed to save the UserSettings to json file.", e)).LogError();
            }

            return Result.Ok().WithSuccess("UserSettings were saved successfully!").LogInformation();
        }

        /// <inheritdoc/>
        public void UpdateSettings(ISettingsModel sourceSettings, bool saveAfterUpdate = true)
        {
            _allowSave = false;

            SettingsModel settingsModel = (SettingsModel)sourceSettings;

            // Get a list of all properties in the sourceSettings.
            settingsModel.GetType().GetProperties().Where(x => x.CanWrite).ToList().ForEach(sourceSettingsProperty =>
            {
                // Check whether target object has the source property, which will always be true due to inheritance.
                var targetSettingsProperty = GetType().GetProperty(sourceSettingsProperty.Name);
                if (targetSettingsProperty != null)
                {
                    // Now copy the value to the matching property in this UserSettings instance.
                    var value = sourceSettingsProperty.GetValue(settingsModel, null);
                    if (value != null)
                    {
                        GetType().GetProperty(sourceSettingsProperty.Name).SetValue(this, value, null);
                    }
                    else
                    {
                        Log.Debug($"Value was read from jsonSettings as null for property {targetSettingsProperty}, will maintain default value.");
                    }
                }
            });

            _allowSave = true;
            if (saveAfterUpdate)
            {
                Save();
            }
        }

        #endregion

        #endregion
    }
}