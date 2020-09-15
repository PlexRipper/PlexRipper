using Newtonsoft.Json;
using PlexRipper.Application.Settings.Models.Base;

namespace PlexRipper.Application.Settings.Models
{
    public class UserInterfaceSettingsModel : BaseModel
    {
        private ConfirmationSettingsModel _confirmationSettings = new ConfirmationSettingsModel();

        #region Properties

        public virtual ConfirmationSettingsModel ConfirmationSettings
        {
            get => _confirmationSettings;
            set
            {
                _confirmationSettings = value;
                _confirmationSettings.PropertyChanged += (sender, args) => OnPropertyChanged();
            }
        }

        #endregion Properties
    }
}