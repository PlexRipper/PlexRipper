using Newtonsoft.Json;
using PlexRipper.DownloadManager;

namespace PlexRipper.WebAPI.SignalR.Common;

public class ServerDownloadProgressDTO
{
    /// <summary>
    /// Gets or sets the <see cref="PlexServer"/> Id.
    /// </summary>
    [JsonProperty("id", Required = Required.Always)]
    public int Id { get; set; }

    [JsonProperty("downloads", Required = Required.Always)]
    public List<DownloadProgressDTO> Downloads { get; set; }
}