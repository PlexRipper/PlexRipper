using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO.FolderPath
{
    public class FolderPathDTO
    {
        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty("displayName", Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty("directory", Required = Required.Always)]
        public string Directory { get; set; }

    }
}