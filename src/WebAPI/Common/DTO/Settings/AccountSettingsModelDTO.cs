using Newtonsoft.Json;
using PlexRipper.Application.Settings.Models.Base;

namespace PlexRipper.Application.Settings.Models
{
    public class AccountSettingsModelDTO
    {
        #region Properties

        [JsonProperty("activeAccountId", Required = Required.Always)]
        public int ActiveAccountId { get; set; }

        #endregion Properties
    }
}