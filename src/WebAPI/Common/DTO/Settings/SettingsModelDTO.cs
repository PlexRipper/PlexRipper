using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class SettingsModelDTO
    {
        [JsonProperty(Required = Required.Always)]
        public bool FirstTimeSetup { get; set; }

        [JsonProperty(Required = Required.Always)]
        public AccountSettingsModelDTO AccountSettings { get; set; }

        [JsonProperty(Required = Required.Always)]
        public AdvancedSettingsModelDTO AdvancedSettings { get; set; }

        [JsonProperty(Required = Required.Always)]
        public UserInterfaceSettingsModelDTO UserInterfaceSettings { get; set; }
    }
}