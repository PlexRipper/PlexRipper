using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class DownloadProgressDTO
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("percentage")]
        public decimal Percentage { get; set; }

        [JsonProperty("dataReceived")]
        public string DataReceived { get; set; }

        [JsonProperty("dataTotal")]
        public string DataTotal { get; set; }

        [JsonProperty("downloadSpeed")]
        public string DownloadSpeed { get; set; }
    }
}
