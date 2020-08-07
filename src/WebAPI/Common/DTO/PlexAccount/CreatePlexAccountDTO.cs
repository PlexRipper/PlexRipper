using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class
        CreatePlexAccountDTO
    {
        [JsonProperty("displayName", Required = Required.DisallowNull)]
        public string DisplayName { get; set; }

        [JsonProperty("username", Required = Required.DisallowNull)]
        public string Username { get; set; }

        [JsonProperty("password", Required = Required.DisallowNull)]
        public string Password { get; set; }

        [JsonProperty("isEnabled", Required = Required.DisallowNull)]
        public bool IsEnabled { get; set; }
    }
}
