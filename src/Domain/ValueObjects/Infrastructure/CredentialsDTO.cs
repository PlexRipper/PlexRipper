using Newtonsoft.Json;

namespace PlexRipper.Domain.ValueObjects
{
    public class CredentialsDTO
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
