using PlexRipper.Application.Common;
using PlexRipper.Application.Settings.Models.Base;

namespace PlexRipper.Application.Settings.Models
{
    /// <summary>
    /// Used to model the settings, which is then used to serialize to json.
    /// </summary>
    public class SettingsModel : BaseModel, ISettingsModel
    {
        #region Properties

        public virtual AccountSettingsModel AccountSettings { get; set; } = new AccountSettingsModel();

        public virtual AdvancedSettingsModel AdvancedSettings { get; set; } = new AdvancedSettingsModel();

        public virtual UserInterfaceSettingsModel UserInterfaceSettings { get; set; } = new UserInterfaceSettingsModel();

        #endregion Properties
    }
}