using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class DownloadProgressDTO
    {
        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty("status", Required = Required.Always)]
        public string Status { get; set; }

        [JsonProperty("percentage", Required = Required.Always)]
        public decimal Percentage { get; set; }

        [JsonProperty("dataReceived", Required = Required.Always)]
        public string DataReceived { get; set; }

        [JsonProperty("dataTotal", Required = Required.Always)]
        public string DataTotal { get; set; }

        [JsonProperty("downloadSpeed", Required = Required.Always)]
        public string DownloadSpeed { get; set; }
    }
}