using Newtonsoft.Json;

namespace PlexRipper.Domain.Settings
{
    /// <summary>
    /// Used to model the settings, which is then used to serialize to json.
    /// </summary>
    public class SettingsModel : BaseModel
    {
        #region Fields

        private bool _confirmExit = false;
        private int _activeAccountId = 0;

        #endregion Fields

        #region Properties

        [JsonProperty("activeAccountId", Required = Required.Always)]
        public int ActiveAccountId
        {
            get => _activeAccountId;
            set
            {
                if (value != _activeAccountId)
                {
                    _activeAccountId = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonProperty("advancedSettings", Required = Required.Always)]
        public AdvancedSettingsModel AdvancedSettings { get; set; } = new AdvancedSettingsModel();

        #endregion Properties
    }
}