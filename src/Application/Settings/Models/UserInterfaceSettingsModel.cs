using Newtonsoft.Json;
using PlexRipper.Application.Settings.Models.Base;

namespace PlexRipper.Application.Settings.Models
{
    public class UserInterfaceSettingsModel : BaseModel
    {
        private ConfirmationSettingsModel _confirmationSettings = new ConfirmationSettingsModel();

        private DisplaySettingsModel _displaySettings = new DisplaySettingsModel();

        private DateTimeModel _dateTimeSettings = new DateTimeModel();

        #region Properties

        [JsonProperty("confirmationSettings", Required = Required.Always)]
        public ConfirmationSettingsModel ConfirmationSettings
        {
            get => _confirmationSettings;
            set
            {
                if (value != null)
                {
                    _confirmationSettings = value;
                    _confirmationSettings.PropertyChanged += (sender, args) => OnPropertyChanged();
                }
            }
        }

        [JsonProperty("displaySettings", Required = Required.Always)]
        public DisplaySettingsModel DisplaySettings
        {
            get => _displaySettings;
            set
            {
                if (value != null)
                {
                    _displaySettings = value;
                    _displaySettings.PropertyChanged += (sender, args) => OnPropertyChanged();
                }
            }
        }

        [JsonProperty("dateTimeSettings", Required = Required.Always)]
        public DateTimeModel DateTimeSettings
        {
            get => _dateTimeSettings;
            set
            {
                if (value != null)
                {
                    _dateTimeSettings = value;
                    _dateTimeSettings.PropertyChanged += (sender, args) => OnPropertyChanged();
                }
            }
        }

        #endregion Properties
    }
}