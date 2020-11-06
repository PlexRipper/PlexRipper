using Newtonsoft.Json;

namespace PlexRipper.Application.Settings.Models
{
    public class UserInterfaceSettingsModelDTO
    {
        #region Properties

        [JsonProperty("confirmationSettings", Required = Required.Always)]
        public ConfirmationSettingsModelDTO ConfirmationSettings { get; set; }

        [JsonProperty("displaySettings", Required = Required.Always)]
        public DisplaySettingsModelDTO DisplaySettings { get; set; }

        #endregion Properties
    }
}