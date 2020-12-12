using Newtonsoft.Json;
using PlexRipper.Domain;

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

        [JsonProperty("status", Required = Required.Always)]
        public DownloadStatus Status { get; set; }

        /// <summary>
        /// The relative obfuscated URL of the media to be downloaded, e.g: /library/parts/47660/156234666/file.mkv
        /// </summary>
        [JsonProperty("fileLocationUrl", Required = Required.Always)]
        public string FileLocationUrl { get; set; }

        [JsonProperty("FileName", Required = Required.Always)]
        public string FileName { get; set; }

        /// <summary>
        /// If this type is an episode of a tv show then this will be the title of that tv show.
        /// </summary>
        [JsonProperty("titleTvShow", Required = Required.Always)]
        public string TitleTvShow { get; set; }

        /// <summary>
        /// If this type is an episode of a tv show then this will be the title of that tv show season.
        /// </summary>
        [JsonProperty("titleTvShowSeason", Required = Required.Always)]
        public string TitleTvShowSeason { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        public PlexMediaType MediaType { get; set; }

        /// <summary>
        /// The identifier used by Plex to keep track of media.
        /// </summary>
        [JsonProperty("ratingKey", Required = Required.Always)]
        public int RatingKey { get; set; }

        [JsonProperty("dataReceived", Required = Required.Always)]
        public long DataReceived { get; set; }

        [JsonProperty("dataTotal", Required = Required.Always)]
        public long DataTotal { get; set; }

        /// <summary>
        /// The download priority, the higher the more important.
        /// </summary>
        [JsonProperty("priority", Required = Required.Always)]
        public int Priority { get; set; }

        [JsonProperty("plexServerId", Required = Required.Always)]
        public int PlexServerId { get; set; }

        [JsonProperty("plexLibraryId", Required = Required.Always)]
        public int PlexLibraryId { get; set; }
    }
}