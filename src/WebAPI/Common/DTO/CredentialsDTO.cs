using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class CredentialsDTO
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
