using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class DownloadManagerModelDTO
    {
        [JsonProperty(Required = Required.Always)]
        public int DownloadSegments { get; set; }
    }
}