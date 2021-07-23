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

        private AccountSettingsModel _accountSettings = new();

        private UserInterfaceSettingsModel _userInterfaceSettings = new();

        private AdvancedSettingsModel _advancedSettings = new();

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
                    _accountSettings.PropertyChanged += (_, _) => OnPropertyChanged();
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
                    _advancedSettings.PropertyChanged += (_, _) => OnPropertyChanged();
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
                    _userInterfaceSettings.PropertyChanged += (_, _) => OnPropertyChanged();
                }
            }
        }

        #endregion Properties
    }
}