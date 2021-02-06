using Newtonsoft.Json;
using PlexRipper.Domain;

namespace PlexRipper.DownloadManager.Common
{
    /// <summary>
    /// Used to send any <see cref="DownloadStatus"/> changes to the front-end.
    /// </summary>
    public class DownloadStatusChanged
    {
        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty("status", Required = Required.Always)]
        public DownloadStatus Status { get; set; }

        [JsonProperty("plexServerId", Required = Required.Always)]
        public int PlexServerId { get; set; }

        [JsonProperty("plexLibraryId", Required = Required.Always)]
        public int PlexLibraryId { get; set; }
    }
}