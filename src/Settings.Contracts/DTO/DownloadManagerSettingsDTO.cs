using Newtonsoft.Json;

namespace Settings.Contracts;

public class DownloadManagerSettingsDTO : IDownloadManagerSettings
{
    [JsonProperty(Required = Required.Always)]
    public int DownloadSegments { get; set; }
}