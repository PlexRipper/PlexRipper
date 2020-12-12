using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class CredentialsDTO
    {
        [JsonProperty("username", Required = Required.DisallowNull)]
        public string Username { get; set; }

        [JsonProperty("password", Required = Required.DisallowNull)]
        public string Password { get; set; }
    }
}