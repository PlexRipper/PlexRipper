using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class AdvancedSettingsModelDTO
    {
        [JsonProperty(Required = Required.Always)]
        public DownloadManagerModelDTO DownloadManager { get; set; }
    }
}