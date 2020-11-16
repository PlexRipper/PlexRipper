using PlexRipper.Application.Settings.Models.Base;

namespace PlexRipper.Application.Settings.Models
{
    public class UserInterfaceSettingsModel : BaseModel
    {
        private ConfirmationSettingsModel _confirmationSettings = new ConfirmationSettingsModel();

        private DisplaySettingsModel _displaySettings = new DisplaySettingsModel();

        private DateTimeModel _dateTimeSettings = new DateTimeModel();

        #region Properties

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