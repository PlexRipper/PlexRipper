using PlexRipper.Application.Settings.Models.Base;

namespace PlexRipper.Application.Settings.Models
{
    public class UserInterfaceSettingsModel : BaseModel
    {
        private ConfirmationSettingsModel _confirmationSettings = new ConfirmationSettingsModel();
        private DisplaySettingsModel _displaySettings = new DisplaySettingsModel();

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

        #endregion Properties
    }
}