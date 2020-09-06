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
        public int Id { get; }

        [JsonProperty("status", Required = Required.Always)]
        public DownloadStatus Status { get; }

        public DownloadStatusChanged(int id, DownloadStatus downloadStatus)
        {
            Id = id;
            Status = downloadStatus;
        }
    }
}