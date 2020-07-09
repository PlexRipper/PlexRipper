using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class DownloadTaskDTO
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

    }
}
