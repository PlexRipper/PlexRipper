using Newtonsoft.Json;

namespace PlexRipper.Application.Common
{
    public class AuthPinErrors
    {
        [JsonProperty(Required = Required.Always)]
        public int Code { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Message { get; set; }
    }
}