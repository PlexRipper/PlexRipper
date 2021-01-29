using Newtonsoft.Json;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common.DTO.WebApi
{
    public class DownloadTaskMediaDTO
    {
        /// <summary>
        /// The identifier used by Plex to keep track of media.
        /// </summary>
        [JsonProperty("key", Required = Required.Always)]
        public int Key { get; set; }

        /// <summary>
        /// The formatted media title as shown in Plex.
        /// </summary>
        [JsonProperty("title", Required = Required.Always)]
        public string Title { get; set; }

        /// <summary>
        /// Note: Naming third just 'type' will cause errors in the Typescript type generating.
        /// </summary>
        [JsonProperty("mediaType", Required = Required.Always)]
        public virtual PlexMediaType MediaType => PlexMediaType.None;

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