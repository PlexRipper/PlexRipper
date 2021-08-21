using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class AccountSettingsModelDTO
    {
        [JsonProperty(Required = Required.Always)]
        public int ActiveAccountId { get; set; }
    }
}