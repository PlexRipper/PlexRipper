using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlexRipper.Settings.Models;
using System;
using System.IO;

namespace PlexRipper.Settings
{
    public class UserSettings
    {
        public static string FileName { get; } = "PlexRipperSettings.json";
        public static string FilePath { get; } = System.IO.Directory.GetCurrentDirectory();

        public static string ConfigDirectory => Path.Join(FilePath, "\\config");
        public static string FileLocation => Path.Join(ConfigDirectory, FileName);

        private SettingsModel _settings;

        public SettingsModel Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                _settings.PropertyChanged += (sender, args) => Save();
            }
        }

        public UserSettings()
        {
            Settings = new SettingsModel();
        }

        public bool Save()
        {
            CreateConfigDirectory();

            try
            {
                var jObject = JObject.FromObject(Settings);

                using (StreamWriter file = File.CreateText(FileLocation))
                using (JsonTextWriter writer = new JsonTextWriter(file)
                {
                    Formatting = Formatting.Indented
                })
                {
                    jObject.WriteTo(writer);
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public bool Load()
        {
            if (!File.Exists(FileLocation))
            {
                Save();
            }

            try
            {
                // read JSON directly from a file
                using (StreamReader file = File.OpenText(FileLocation))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JObject o2 = (JObject)JToken.ReadFrom(reader);
                    Settings = o2.ToObject<SettingsModel>();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return true;
        }

        public static bool Reset()
        {
            return true;
        }

        public void CreateConfigDirectory()
        {
            if (!Directory.Exists(ConfigDirectory))
            {
                Directory.CreateDirectory(ConfigDirectory);
            }
        }
    }
}
