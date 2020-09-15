using Newtonsoft.Json;
using PlexRipper.Application.Settings.Models.Base;

namespace PlexRipper.Application.Settings.Models
{
    public class UserInterfaceSettingsModel : BaseModel
    {
        #region Properties

        public virtual ConfirmationSettingsModel Confirmations { get; set; } = new ConfirmationSettingsModel();

        #endregion Properties
    }
}