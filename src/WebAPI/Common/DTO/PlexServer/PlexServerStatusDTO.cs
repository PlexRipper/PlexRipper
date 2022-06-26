using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class PlexServerStatusDTO
    {
        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty("statusCode", Required = Required.Always)]
        public int StatusCode { get; set; }

        [JsonProperty("isSuccessful", Required = Required.Always)]
        public bool IsSuccessful { get; set; }

        [JsonProperty("statusMessage", Required = Required.Always)]
        public string StatusMessage { get; set; }

        [JsonProperty("lastChecked", Required = Required.Always)]
        public DateTime LastChecked { get; set; }

        [JsonProperty("plexServerId", Required = Required.Always)]
        public int PlexServerId { get; set; }
    }
}