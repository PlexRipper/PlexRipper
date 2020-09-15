using PlexRipper.Application.Common;
using PlexRipper.Application.Settings.Models.Base;

namespace PlexRipper.Application.Settings.Models
{
    /// <summary>
    /// Used to model the settings, which is then used to serialize to json.
    /// </summary>
    public class SettingsModel : BaseModel, ISettingsModel
    {
        private AccountSettingsModel _accountSettings = new AccountSettingsModel();
        private UserInterfaceSettingsModel _userInterfaceSettings = new UserInterfaceSettingsModel();
        private AdvancedSettingsModel _advancedSettings = new AdvancedSettingsModel();

        #region Properties

        public AccountSettingsModel AccountSettings
        {
            get => _accountSettings;
            set
            {
                // During every settings load this needs to be subscribed to again
                _accountSettings = value;
                _accountSettings.PropertyChanged += (sender, args) => OnPropertyChanged();
            }
        }

        public AdvancedSettingsModel AdvancedSettings
        {
            get => _advancedSettings;
            set
            {
                // During every settings load this needs to be subscribed to again
                _advancedSettings = value;
                _advancedSettings.PropertyChanged += (sender, args) => OnPropertyChanged();
            }
        }

        public UserInterfaceSettingsModel UserInterfaceSettings
        {
            get => _userInterfaceSettings;
            set
            {
                // During every settings load this needs to be subscribed to again
                _userInterfaceSettings = value;
                _userInterfaceSettings.PropertyChanged += (sender, args) => OnPropertyChanged();
            }
        }

        #endregion Properties
    }
}