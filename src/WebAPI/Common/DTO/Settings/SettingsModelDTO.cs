using Newtonsoft.Json;
using PlexRipper.Application.Settings.Models;

namespace PlexRipper.WebAPI.Common.DTO.Settings
{
    /// <summary>
    /// DTOs are used to enforce an always required attribute.
    /// Due to this conflicting with the <see cref="SettingsModel"/> used to read the JSON Settings, since those properties are optional.
    /// </summary>
    public class SettingsModelDTO
    {
        [JsonProperty("accountSettings", Required = Required.Always)]
        public AccountSettingsModelDTO AccountSettings { get; set; }

        [JsonProperty("advancedSettings", Required = Required.Always)]
        public AdvancedSettingsModelDTO AdvancedSettings { get; set; }

        [JsonProperty("userInterfaceSettings", Required = Required.Always)]
        public UserInterfaceSettingsModelDTO UserInterfaceSettings { get; set; }
    }
}