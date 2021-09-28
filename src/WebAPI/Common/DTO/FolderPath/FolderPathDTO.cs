using Newtonsoft.Json;
using PlexRipper.Domain;

namespace PlexRipper.WebAPI.Common.DTO.FolderPath
{
    public class FolderPathDTO
    {
        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty("folderType", Required = Required.Always)]
        public FolderType FolderType { get; set; }

        [JsonProperty("mediaType", Required = Required.Always)]
        public PlexMediaType MediaType { get; set; }

        [JsonProperty("displayName", Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty("directory", Required = Required.Always)]
        public string Directory { get; set; }

        [JsonProperty("isValid", Required = Required.Always)]
        public bool IsValid { get; set; }
    }
}