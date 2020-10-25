using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO.PlexMediaData
{
    public class PlexMovieDataPartDTO
    {
        #region Properties

        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty("Key", Required = Required.Always)]
        public string Key { get; set; }

        [JsonProperty("Duration", Required = Required.Always)]
        public int Duration { get; set; }

        [JsonProperty("File", Required = Required.Always)]
        public string File { get; set; }

        [JsonProperty("Size", Required = Required.Always)]
        public long Size { get; set; }

        [JsonProperty("Container", Required = Required.Always)]
        public string Container { get; set; }

        [JsonProperty("VideoProfile", Required = Required.Always)]
        public string VideoProfile { get; set; }

        #endregion
    }
}