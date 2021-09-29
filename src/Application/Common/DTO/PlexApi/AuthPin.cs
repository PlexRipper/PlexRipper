using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PlexRipper.Application.Common
{
    public class AuthPin
    {
        [JsonProperty(Required = Required.Always)]
        public List<AuthPinErrors> Errors { get; set; }

        [JsonProperty(Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Code { get; set; }

        [JsonProperty(Required = Required.Always)]
        public bool Trusted { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string ClientIdentifier { get; set; }

        [JsonProperty(Required = Required.Always)]
        public AuthPinLocation Location { get; set; }

        [JsonProperty(Required = Required.Always)]
        public int ExpiresIn { get; set; }

        [JsonProperty(Required = Required.Always)]
        public DateTime CreatedAt { get; set; }

        [JsonProperty(Required = Required.Always)]
        public DateTime ExpiresAt { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string AuthToken { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string NewRegistration { get; set; }
    }
}