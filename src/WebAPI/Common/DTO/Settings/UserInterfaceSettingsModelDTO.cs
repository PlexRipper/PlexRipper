using Newtonsoft.Json;
using PlexRipper.Application.Settings.Models.Base;

namespace PlexRipper.Application.Settings.Models
{
    public class UserInterfaceSettingsModelDTO
    {
        #region Properties

        [JsonProperty("confirmations", Required = Required.Always)]
        public ConfirmationSettingsModelDTO Confirmations { get; set; } = new ConfirmationSettingsModelDTO();

        #endregion Properties
    }
}