using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentResults;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PlexRipper.Application.Common;
using PlexRipper.Application.Settings.Models;
using PlexRipper.Domain;

namespace PlexRipper.Settings
{
    /// <inheritdoc cref="IUserSettings"/>
    public class UserSettings : SettingsModel, IUserSettings
    {
        #region Fields

        private readonly IFileSystem _fileSystem;

        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Populate,
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter(),
            },
        };

        private bool _allowSave = true;

        #endregion

        #region Constructors

        public UserSettings(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        #endregion

        #region Properties

        [JsonIgnore]
        private static string FileName { get; } = "PlexRipperSettings.json";

        [JsonIgnore]
        private string FileLocation => Path.Join(_fileSystem.ConfigDirectory, FileName);

        #endregion

        #region Methods

        #region Public

        public Result<bool> Setup()
        {
            // TODO Add result based error handeling here
            Log.Information("Setting up UserSettings");
            if (!File.Exists(FileLocation))
            {
                Log.Information($"{FileName} doesn't exist, will create new one now.");
                string jsonString = JsonConvert.SerializeObject(new SettingsModel(), _jsonSerializerSettings);
                File.WriteAllText(FileLocation, jsonString);
            }

            Load();

            PropertyChanged += (sender, args) => Save();

            return Result.Ok(true);
        }

        public bool Load()
        {
            Log.Information("Loading UserSettings now.");

            try
            {
                string jsonString = File.ReadAllText(FileLocation);
                var loadedSettings = JsonConvert.DeserializeObject<SettingsModel>(jsonString, _jsonSerializerSettings);
                UpdateSettings(loadedSettings, false);
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to load the UserSettings to json file.");
                throw;
            }

            Log.Information("UserSettings were loaded successfully!");
            return true;
        }

        public void Reset()
        {
            Log.Debug("Resetting UserSettings");

            UpdateSettings(new SettingsModel());
            Save();
        }

        /// <inheritdoc/>
        public bool Save()
        {
            if (!_allowSave)
            {
                Log.Warning("UserSettings is denied from saving by the allowSave lock");
                return false;
            }

            Log.Information("Saving UserSettings now.");

            try
            {
                string jsonString = JsonConvert.SerializeObject(this, _jsonSerializerSettings);
                File.WriteAllText(FileLocation, jsonString);
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to save the UserSettings to json file.");
                throw;
            }

            Log.Information("UserSettings were saved successfully!");
            return true;
        }

        /// <inheritdoc/>
        public void UpdateSettings(SettingsModel sourceSettings, bool saveAfterUpdate = true)
        {
            _allowSave = false;

            // Get a list of all properties in the sourceSettings.
            sourceSettings.GetType().GetProperties().Where(x => x.CanWrite).ToList().ForEach(sourceSettingsProperty =>
            {
                // Check whether target object has the source property, which will always be true due to inheritance.
                var targetSettingsProperty = GetType().GetProperty(sourceSettingsProperty.Name);
                if (targetSettingsProperty != null)
                {
                    // Now copy the value to the matching property in this UserSettings instance.
                    var value = sourceSettingsProperty.GetValue(sourceSettings, null);
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