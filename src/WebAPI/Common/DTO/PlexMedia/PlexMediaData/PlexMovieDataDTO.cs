using System.Collections.Generic;
using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO.PlexMediaData
{
    public class PlexMovieDataDTO
    {
        #region Properties

        [JsonProperty("id", Required = Required.Always)]

        public int Id { get; set; }

        [JsonProperty("mediaFormat", Required = Required.Always)]

        public string MediaFormat { get; set; }

        [JsonProperty("duration", Required = Required.Always)]
        public long Duration { get; set; }

        [JsonProperty("videoResolution", Required = Required.Always)]
        public string VideoResolution { get; set; }

        [JsonProperty("width", Required = Required.Always)]
        public int Width { get; set; }

        [JsonProperty("height", Required = Required.Always)]
        public int Height { get; set; }

        [JsonProperty("bitrate", Required = Required.Always)]
        public int Bitrate { get; set; }

        [JsonProperty("videoCodec", Required = Required.Always)]
        public string VideoCodec { get; set; }

        [JsonProperty("videoFrameRate", Required = Required.Always)]
        public string VideoFrameRate { get; set; }

        [JsonProperty("aspectRatio", Required = Required.Always)]
        public double AspectRatio { get; set; }

        [JsonProperty("videoProfile", Required = Required.Always)]
        public string VideoProfile { get; set; }

        [JsonProperty("audioProfile", Required = Required.Always)]
        public string AudioProfile { get; set; }

        [JsonProperty("audioCodec", Required = Required.Always)]
        public string AudioCodec { get; set; }

        [JsonProperty("audioChannels", Required = Required.Always)]
        public int AudioChannels { get; set; }

        [JsonProperty("parts", Required = Required.Always)]
        public List<PlexMovieDataPartDTO> Parts { get; set; }

        #endregion
    }
}