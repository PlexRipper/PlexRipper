using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Common.DTO
{
    public class CredentialsDTO
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}
