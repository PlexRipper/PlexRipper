using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class DownloadTaskDTO
    {
        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        /// <summary>
        /// The formatted media title as shown in Plex.
        /// </summary>
        [JsonProperty("title", Required = Required.Always)]
        public string Title { get; set; }

        /// <summary>
        /// The full media title including the [TvShow]/[Season]/[Episode] as shown in Plex.
        /// </summary>
        [JsonProperty("fullTitle", Required = Required.Always)]
        public string FullTitle { get; set; }

        [JsonProperty("status", Required = Required.Always)]
        public DownloadStatus Status { get; set; }

        /// <summary>
        /// The relative obfuscated URL of the media to be downloaded, e.g: /library/parts/47660/156234666/file.mkv.
        /// </summary>
        [JsonProperty("fileLocationUrl", Required = Required.Always)]
        public string FileLocationUrl { get; set; }

        [JsonProperty("fileName", Required = Required.Always)]
        public string FileName { get; set; }

        /// <summary>
        /// Note: Naming third just 'type' will cause errors in the Typescript type generating.
        /// </summary>
        [JsonProperty("mediaType", Required = Required.Always)]
        public PlexMediaType MediaType { get; set; }

        [JsonProperty("downloadTaskType", Required = Required.Always)]
        public DownloadTaskType DownloadTaskType { get; set; }

        /// <summary>
        /// The identifier used by Plex to keep track of media.
        /// </summary>
        [JsonProperty("key", Required = Required.Always)]
        public int Key { get; set; }

        [JsonProperty("downloadSpeed", Required = Required.Always)]
        public int DownloadSpeed { get; set; }

        [JsonProperty("dataReceived", Required = Required.Always)]
        public long DataReceived { get; set; }

        [JsonProperty("dataTotal", Required = Required.Always)]
        public long DataTotal { get; set; }

        [JsonProperty("percentage", Required = Required.Always)]
        public decimal Percentage { get; set; }

        [JsonProperty("downloadDirectory", Required = Required.Always)]
        public string DownloadDirectory { get; set; }

        [JsonProperty("destinationDirectory", Required = Required.Always)]
        public string DestinationDirectory { get; set; }

        /// <summary>
        /// The download priority, the higher the more important.
        /// </summary>
        [JsonProperty("priority", Required = Required.Always)]
        public int Priority { get; set; }

        #region Relationships

        [JsonProperty("plexServerId", Required = Required.Always)]
        public int PlexServerId { get; set; }

        [JsonProperty("plexLibraryId", Required = Required.Always)]
        public int PlexLibraryId { get; set; }

        [JsonProperty("parentId", Required = Required.Always)]
        public int? ParentId { get; set; }

        #endregion

        [JsonProperty("timeRemaining", Required = Required.Always)]
        public long TimeRemaining { get; set; }

        [JsonProperty("downloadUrl", Required = Required.Always)]
        public string DownloadUrl { get; set; }

        [JsonProperty("quality", Required = Required.Always)]
        public string Quality { get; set; }

        /// <summary>
        /// The nested <see cref="DownloadTask"/> used for seasons and episodes.
        /// "Required = Required.Default" is used for ensuring it's optional in the Typescript generating.
        /// </summary>
        [JsonProperty("children", Required = Required.Default)]
        public List<DownloadTaskDTO> Children { get; set; }

        /// <summary>
        /// The actions that can be taken on this <see cref="DownloadTask"/>.
        /// This is filled by the front-end and depends on the DownloadStatus
        /// </summary>
        [JsonProperty("actions", Required = Required.Always)]
        public string[] Actions { get; set; }
    }
}