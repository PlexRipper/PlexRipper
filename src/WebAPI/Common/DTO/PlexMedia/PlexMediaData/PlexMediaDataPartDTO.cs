using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO.PlexMediaData;

public class PlexMediaDataPartDTO
{
    #region Properties

    [JsonProperty("obfuscatedFilePath", Required = Required.Always)]
    public string ObfuscatedFilePath { get; set; }

    [JsonProperty("Duration", Required = Required.Always)]
    public int Duration { get; set; }

    [JsonProperty("File", Required = Required.Always)]
    public string File { get; set; }

    [JsonProperty("Size", Required = Required.Always)]
    public long Size { get; set; }

    [JsonProperty("Container", Required = Required.Always)]
    public string Container { get; set; }

    [JsonProperty("VideoProfile", Required = Required.Always)]
    public string VideoProfile { get; set; }

    #endregion
}