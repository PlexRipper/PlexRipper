using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class UpdateDefaultDestinationDTO
    {
        [JsonProperty("libraryId", Required = Required.Always)]
        public int LibraryId { get; set; }

        [JsonProperty("folderPathId", Required = Required.Always)]
        public int FolderPathId { get; set; }
    }
}