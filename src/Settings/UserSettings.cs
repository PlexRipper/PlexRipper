using Newtonsoft.Json;
using PlexRipper.Domain;
using PlexRipper.Settings.Models;
using System;
using System.IO;
using System.Linq;
using PlexRipper.Application.Common;

namespace PlexRipper.Settings
{
    public class UserSettings : SettingsModel, IUserSettings
    {
        private readonly IFileSystem _fileSystem;

        #region Properties

        [JsonIgnore]
        private string FileLocation => Path.Join(_fileSystem.ConfigDirectory, FileName);

        [JsonIgnore]
        private static string FileName { get; } = "PlexRipperSettings.json";

        #endregion Properties



        #region Constructors

        public UserSettings(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
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

        private void CreateSettingsFile()
        {
            Log.Information($"{FileName} doesn't exist, will create now.");
            string jsonString = JsonConvert.SerializeObject(new SettingsModel(), Formatting.Indented);
            File.WriteAllText(FileLocation, jsonString);
        }

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
