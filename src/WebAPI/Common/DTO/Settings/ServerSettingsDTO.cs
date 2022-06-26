using Newtonsoft.Json;
using PlexRipper.Application;
using PlexRipper.Domain.DownloadManager;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class ServerSettingsDTO : IServerSettings
    {
        [JsonProperty(Required = Required.Always)]
        public List<PlexServerSettingsModel> Data { get; set; }
    }
}