using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Common.DTO
{
    public class PlexAuthenticationDTO
    {
        [JsonPropertyName("user")]
        public PlexAccountDTO User { get; set; }
    }
}
