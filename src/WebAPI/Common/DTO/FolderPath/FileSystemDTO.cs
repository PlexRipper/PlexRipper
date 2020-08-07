using System.Collections.Generic;
using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO.FolderPath
{
    public class FileSystemDTO
    {
        [JsonProperty("parent", Required = Required.Always)]
        public string Parent { get; set; }

        [JsonProperty("directories", Required = Required.Always)]
        public List<FileSystemModelDTO> Directories { get; set; }

        [JsonProperty("files", Required = Required.Always)]
        public List<FileSystemModelDTO> Files { get; set; }
    }
}