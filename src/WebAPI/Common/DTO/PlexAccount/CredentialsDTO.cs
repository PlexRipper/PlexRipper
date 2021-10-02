using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class CredentialsDTO
    {
        [JsonProperty(Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Username { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Password { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string ClientId { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public string VerificationCode { get; set; }
    }
}