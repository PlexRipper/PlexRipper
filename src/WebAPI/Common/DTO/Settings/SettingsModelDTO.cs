using Newtonsoft.Json;
using PlexRipper.Application;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class SettingsModelDTO
    {
        [JsonProperty(Required = Required.Always)]
        public GeneralSettingsDTO GeneralSettings { get; set; }

        [JsonProperty(Required = Required.Always)]
        public ConfirmationSettingsDTO ConfirmationSettings { get; set; }

        [JsonProperty(Required = Required.Always)]
        public DateTimeSettingsDTO DateTimeSettings { get; set; }

        [JsonProperty(Required = Required.Always)]
        public DisplaySettingsDTO DisplaySettings { get; set; }

        [JsonProperty(Required = Required.Always)]
        public DownloadManagerSettingsDTO DownloadManagerSettings { get; set; }

        [JsonProperty(Required = Required.Always)]
        public LanguageSettingsDTO LanguageSettings { get; set; }

        [JsonProperty(Required = Required.Always)]
        public ServerSettingsDTO ServerSettings { get; set; }
    }
}