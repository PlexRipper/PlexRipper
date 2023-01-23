using Newtonsoft.Json;
using PlexRipper.Domain.DownloadManager;

namespace Settings.Contracts;

public class ServerSettingsDTO : IServerSettings
{
    [JsonProperty(Required = Required.Always)]
    public List<PlexServerSettingsModel> Data { get; set; }
}