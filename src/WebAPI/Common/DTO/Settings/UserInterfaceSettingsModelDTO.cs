using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class UserInterfaceSettingsModelDTO
    {
        [JsonProperty(Required = Required.Always)]
        public string Language { get; set; }

        [JsonProperty(Required = Required.Always)]
        public ConfirmationSettingsModelDTO ConfirmationSettings { get; set; }

        [JsonProperty(Required = Required.Always)]
        public DisplaySettingsModelDTO DisplaySettings { get; set; }

        [JsonProperty(Required = Required.Always)]
        public DateTimeModelDTO DateTimeSettings { get; set; }
    }
}