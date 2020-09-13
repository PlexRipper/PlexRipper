using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using PlexRipper.Application.Common;
using PlexRipper.Domain;
using PlexRipper.Domain.Settings;

namespace PlexRipper.Settings
{
    /// <inheritdoc cref="IUserSettings"/>
    public class UserSettings : SettingsModel, IUserSettings
    {
        #region Fields

        private readonly IFileSystem _fileSystem;
        private bool _allowSave = true;

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
            string jsonString = JsonConvert.SerializeObject(new SettingsModel(), Formatting.Indented);
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
                var loadedSettings =
                    JsonConvert.DeserializeObject<SettingsModel>(jsonString);
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
            this.AdvancedSettings = sourceSettings.AdvancedSettings;
            this.ActiveAccountId = sourceSettings.ActiveAccountId;
            _allowSave = true;
            Save();
        }

        #endregion

        #endregion
    }
}