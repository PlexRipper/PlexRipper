using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
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
        private bool _allowSave = true;

        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Populate,
        };

        #endregion

        #region Constructors

        public UserSettings(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            this.PropertyChanged += (sender, args) => Save();

            Load();
        }

        #endregion

        #region Properties

        [JsonIgnore]
        private string FileLocation => Path.Join(_fileSystem.ConfigDirectory, FileName);

        [JsonIgnore]
        private static string FileName { get; } = "PlexRipperSettings.json";

        #endregion

        #region Methods

        #region Private

        private void CreateSettingsFile()
        {
            Log.Information($"{FileName} doesn't exist, will create now.");
            string jsonString = JsonConvert.SerializeObject(new SettingsModel(), _jsonSerializerSettings);
            File.WriteAllText(FileLocation, jsonString);
        }

        #endregion

        #region Public

        public bool Load()
        {
            Log.Debug("Loading UserSettings now.");

            if (!File.Exists(FileLocation))
            {
                CreateSettingsFile();
            }

            try
            {
                string jsonString = File.ReadAllText(FileLocation);
                var loadedSettings = JsonConvert.DeserializeObject<SettingsModel>(jsonString, _jsonSerializerSettings);
                UpdateSettings(loadedSettings);
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to load the UserSettings to json file.");
                throw;
            }

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

            Log.Debug("Saving UserSettings now.");

            try
            {
                string jsonString = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(FileLocation, jsonString);

                return true;
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to save the UserSettings to json file.");
                throw;
            }
        }


        /// <inheritdoc/>
        public void UpdateSettings(SettingsModel sourceSettings)
        {
            _allowSave = false;

            // Get a list of all properties in the sourceSettings.
            sourceSettings.GetType().GetProperties().Where(x => x.CanWrite).ToList().ForEach(sourceSettingsProperty =>
            {
                // Check whether target object has the source property, which will always be true due to inheritance.
                var targetSettingsProperty = this.GetType().GetProperty(sourceSettingsProperty.Name);
                if (targetSettingsProperty != null)
                {
                    // Now copy the value to the matching property in this UserSettings instance.
                    var value = sourceSettingsProperty.GetValue(sourceSettings, null);
                    if (value != null)
                    {
                        this.GetType().GetProperty(sourceSettingsProperty.Name).SetValue(this, value, null);
                    }
                    else
                    {
                        Log.Debug($"Value was read from jsonSettings as null for property {targetSettingsProperty}, will maintain default value.");
                    }
                }
            });

            _allowSave = true;
            Save();
        }

        #endregion

        #endregion
    }
}