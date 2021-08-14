using Newtonsoft.Json;
using PlexRipper.Application.Common;
using PlexRipper.Settings.Models.Base;

namespace PlexRipper.Settings.Models
{
    /// <summary>
    /// Used to model the settings, which is then used to serialize to json.
    /// </summary>
    public class SettingsModel : BaseModel, ISettingsModel
    {
        private bool _firstTimeSetup = true;

        private AccountSettingsModel _accountSettings = new();

        private AdvancedSettingsModel _advancedSettings = new();

        private UserInterfaceSettingsModel _userInterfaceSettings = new();


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
        public IAccountSettingsModel AccountSettings
        {
            get => _accountSettings;
            set
            {
                if (value != null)
                {
                    // During every settings load this needs to be subscribed to again
                    _accountSettings = (AccountSettingsModel) value;
                    _accountSettings.PropertyChanged += (_, _) => OnPropertyChanged();
                }
            }
        }

        [JsonProperty("advancedSettings", Required = Required.Always)]
        public IAdvancedSettingsModel AdvancedSettings
        {
            get => _advancedSettings;
            set
            {
                if (value != null)
                {
                    // During every settings load this needs to be subscribed to again
                    _advancedSettings = (AdvancedSettingsModel) value;
                    _advancedSettings.PropertyChanged += (_, _) => OnPropertyChanged();
                }
            }
        }

        [JsonProperty("userInterfaceSettings", Required = Required.Always)]
        public IUserInterfaceSettingsModel UserInterfaceSettings
        {
            get => _userInterfaceSettings;
            set
            {
                if (value != null)
                {
                    // During every settings load this needs to be subscribed to again
                    _userInterfaceSettings = (UserInterfaceSettingsModel) value;
                    _userInterfaceSettings.PropertyChanged += (_, _) => OnPropertyChanged();
                }
            }
        }

        #endregion Properties
    }
}