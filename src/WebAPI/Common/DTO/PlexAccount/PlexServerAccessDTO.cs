using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO;

public class PlexServerAccessDTO
{
    [JsonProperty("plexServerId", Required = Required.Always)]
    public int PlexServerId { get; set; }

    [JsonProperty("plexLibraryId", Required = Required.Always)]
    public List<int> PlexLibraryIds { get; set; }
}