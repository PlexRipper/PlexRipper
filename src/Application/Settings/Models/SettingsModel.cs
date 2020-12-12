using Newtonsoft.Json;
using PlexRipper.Application.Common;
using PlexRipper.Application.Settings.Models.Base;

namespace PlexRipper.Application.Settings.Models
{
    /// <summary>
    /// Used to model the settings, which is then used to serialize to json.
    /// </summary>
    public class SettingsModel : BaseModel, ISettingsModel
    {
        private bool _firstTimeSetup = true;

        private AccountSettingsModel _accountSettings = new AccountSettingsModel();

        private UserInterfaceSettingsModel _userInterfaceSettings = new UserInterfaceSettingsModel();

        private AdvancedSettingsModel _advancedSettings = new AdvancedSettingsModel();

        #region Properties

        [JsonProperty("firstTimeSetup", Required = Required.Always)]
        public bool FirstTimeSetup
        {
            get => _firstTimeSetup;
            set
            {
                if (value != _firstTimeSetup)
                {
                    _firstTimeSetup = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonProperty("accountSettings", Required = Required.Always)]
        public AccountSettingsModel AccountSettings
        {
            get => _accountSettings;
            set
            {
                if (value != null)
                {
                    // During every settings load this needs to be subscribed to again
                    _accountSettings = value;
                    _accountSettings.PropertyChanged += (sender, args) => OnPropertyChanged();
                }
            }
        }

        [JsonProperty("advancedSettings", Required = Required.Always)]
        public AdvancedSettingsModel AdvancedSettings
        {
            get => _advancedSettings;
            set
            {
                if (value != null)
                {
                    // During every settings load this needs to be subscribed to again
                    _advancedSettings = value;
                    _advancedSettings.PropertyChanged += (sender, args) => OnPropertyChanged();
                }
            }
        }

        [JsonProperty("userInterfaceSettings", Required = Required.Always)]
        public UserInterfaceSettingsModel UserInterfaceSettings
        {
            get => _userInterfaceSettings;
            set
            {
                if (value != null)
                {
                    // During every settings load this needs to be subscribed to again
                    _userInterfaceSettings = value;
                    _userInterfaceSettings.PropertyChanged += (sender, args) => OnPropertyChanged();
                }
            }
        }

        #endregion Properties
    }
}