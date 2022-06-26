using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO.FolderPath;

public class FileSystemModelDTO
{
    [JsonProperty("type", Required = Required.Always)]
    public FileSystemEntityType Type { get; set; }

    [JsonProperty("name", Required = Required.Always)]
    public string Name { get; set; }

    [JsonProperty("path", Required = Required.Always)]
    public string Path { get; set; }

    [JsonProperty("extension", Required = Required.Always)]
    public string Extension { get; set; }

    [JsonProperty("size", Required = Required.Always)]
    public long Size { get; set; }

    [JsonProperty("lastModified", Required = Required.AllowNull)]
    public DateTime? LastModified { get; set; }
}