using Newtonsoft.Json;
using PlexRipper.Application.Settings.Models.Base;

namespace PlexRipper.Application.Settings.Models
{
    public class UserInterfaceSettingsModelDTO
    {
        #region Properties

        [JsonProperty("confirmationSettings", Required = Required.Always)]
        public ConfirmationSettingsModelDTO ConfirmationSettings { get; set; } = new ConfirmationSettingsModelDTO();

        #endregion Properties
    }
}