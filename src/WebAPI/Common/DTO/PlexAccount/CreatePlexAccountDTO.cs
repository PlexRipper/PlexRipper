using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class CreatePlexAccountDTO
    {
        [JsonProperty("displayName", Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty("username", Required = Required.Always)]
        public string Username { get; set; }

        [JsonProperty("password", Required = Required.Always)]
        public string Password { get; set; }

        [JsonProperty("isEnabled", Required = Required.Always)]
        public bool IsEnabled { get; set; }

        [JsonProperty("isMain", Required = Required.Always)]
        public bool IsMain { get; set; }
    }
}