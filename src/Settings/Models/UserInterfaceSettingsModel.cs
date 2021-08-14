using Newtonsoft.Json;
using PlexRipper.Application.Common;
using PlexRipper.Settings.Models.Base;

namespace PlexRipper.Settings.Models
{
    public class UserInterfaceSettingsModel : BaseModel, IUserInterfaceSettingsModel
    {
        private ConfirmationSettingsModel _confirmationSettings = new();

        private DisplaySettingsModel _displaySettings = new();

        private DateTimeModel _dateTimeSettings = new();

        #region Properties

        [JsonProperty("confirmationSettings", Required = Required.Always)]
        public IConfirmationSettingsModel ConfirmationSettings
        {
            get => _confirmationSettings;
            set
            {
                if (value != null)
                {
                    _confirmationSettings = (ConfirmationSettingsModel) value;
                    _confirmationSettings.PropertyChanged += (_, _) => OnPropertyChanged();
                }
            }
        }

        [JsonProperty("displaySettings", Required = Required.Always)]
        public IDisplaySettingsModel DisplaySettings
        {
            get => _displaySettings;
            set
            {
                if (value != null)
                {
                    _displaySettings = (DisplaySettingsModel) value;
                    _displaySettings.PropertyChanged += (_, _) => OnPropertyChanged();
                }
            }
        }

        [JsonProperty("dateTimeSettings", Required = Required.Always)]
        public IDateTimeModel DateTimeSettings
        {
            get => _dateTimeSettings;
            set
            {
                if (value != null)
                {
                    _dateTimeSettings = (DateTimeModel) value;
                    _dateTimeSettings.PropertyChanged += (_, _) => OnPropertyChanged();
                }
            }
        }

        #endregion Properties
    }
}