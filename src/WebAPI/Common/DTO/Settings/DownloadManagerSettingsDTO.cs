using Newtonsoft.Json;
using PlexRipper.Application;

namespace PlexRipper.WebAPI.Common.DTO;

public class DownloadManagerSettingsDTO : IDownloadManagerSettings
{
    [JsonProperty(Required = Required.Always)]
    public int DownloadSegments { get; set; }
}