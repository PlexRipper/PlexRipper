using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace PlexRipper.Settings
{
    public class UserSettings
    {
        public static string FileName { get; } = "PlexRipperSettings.json";
        public static string FilePath { get; } = System.IO.Directory.GetCurrentDirectory();

        public static string FileLocation => Path.Combine(FilePath, FileName);

        public static SettingsModel SettingsModel { get; set; }

        public UserSettings()
        {

        }

        public bool Save()
        {
            try
            {
                string result = JsonConvert.SerializeObject(SettingsModel);

                File.WriteAllText(FileLocation, result);

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
                SettingsModel = new SettingsModel();
                Save();
            }

            try
            {
                // read JSON directly from a file
                using (StreamReader file = File.OpenText(FileLocation))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JObject o2 = (JObject)JToken.ReadFrom(reader);
                    SettingsModel = o2.ToObject<SettingsModel>();
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
    }
}
