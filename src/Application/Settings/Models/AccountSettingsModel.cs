using PlexRipper.Application.Settings.Models.Base;

namespace PlexRipper.Application.Settings.Models
{
    public class AccountSettingsModel : BaseModel
    {
        #region Fields

        private int _activeAccountId = 0;

        #endregion

        #region Properties

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

        #endregion Properties
    }
}