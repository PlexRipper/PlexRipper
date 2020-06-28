using Newtonsoft.Json;
using PlexRipper.Application.Common.Interfaces.Settings;
using PlexRipper.Settings.Models;
using Serilog;
using System;
using System.IO;
using System.Linq;

namespace PlexRipper.Settings
{
    public class UserSettings : SettingsModel, IUserSettings
    {

        #region Properties

        [JsonIgnore]
        private static string ConfigDirectory => Path.Join(FilePath, "\\config");

        [JsonIgnore]
        private static string FileLocation => Path.Join(ConfigDirectory, FileName);

        [JsonIgnore]
        private static string FileName { get; } = "PlexRipperSettings.json";
        [JsonIgnore]
        private static string FilePath { get; } = Directory.GetCurrentDirectory();

        #endregion Properties

        private ILogger Log { get; }

        #region Constructors

        public UserSettings(ILogger logger)
        {
            Log = logger.ForContext<UserSettings>();

            this.PropertyChanged += (sender, args) => Save();

            Load();
        }

        #endregion Constructors

        #region Methods

        public void Reset()
        {
            Log.Debug("Resetting UserSettings");

            SetValues(new SettingsModel());
            Save();
        }

        private void CreateConfigDirectory()
        {
            if (!Directory.Exists(ConfigDirectory))
            {
                Log.Debug("Config directory doesn't exist, will create now.");

                Directory.CreateDirectory(ConfigDirectory);
            }
        }

        public bool Load()
        {
            Log.Debug("Loading UserSettings now.");

            if (!File.Exists(FileLocation))
            {
                Log.Debug($"{FileName} doesn't exist, will create now.");

                Save();
            }

            try
            {
                string jsonString = File.ReadAllText(FileLocation);
                var loadedSettings =
                    JsonConvert.DeserializeObject<SettingsModel>(jsonString);
                SetValues(loadedSettings);
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to load the UserSettings to json file.");
                throw;
            }
            return true;
        }

        public bool Save()
        {
            Log.Debug("Saving UserSettings now.");
            CreateConfigDirectory();

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

        /// <summary>
        /// This will copy values from the sourceSettings and set them to this <see cref="UserSettings"/> instance through reflection. The <see cref="UserSettings"/> also inherits from <see cref="SettingsModel"/> such that we can simply do "userSettings.ApiKey" instead of having a separate instance of the <see cref="SettingsModel"/> in the <see cref="UserSettings"/>.
        /// </summary>
        /// <param name="sourceSettings">The values to be used to set this <see cref="UserSettings"/> instance.</param>
        public void SetValues(SettingsModel sourceSettings)
        {
            // Get a list of all properties in the sourceSettings.
            sourceSettings.GetType().GetProperties().Where(x => x.CanWrite).ToList().ForEach(sourceSettingsProperty =>
            {
                // check whether target object has the source property, which will always be true due to inheritance. 
                var targetSettingsProperty = this.GetType().GetProperty(sourceSettingsProperty.Name);
                if (targetSettingsProperty != null)
                {
                    // Now copy the value to the matching property in this UserSettings instance.
                    var value = sourceSettingsProperty.GetValue(sourceSettings, null);
                    this.GetType().GetProperty(sourceSettingsProperty.Name).SetValue(this, value, null);
                }
            });
        }

        #endregion Methods
    }
}
