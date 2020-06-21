
namespace PlexRipper.Settings.Models
{
    public class SettingsModel : BaseModel
    {
        #region Fields

        private string _apiKey = "bnrijbrogbwoeibgoiuweb";
        private bool _confirmExit = false;

        #endregion Fields

        #region Properties

        public string ApiKey
        {
            get => _apiKey;
            set
            {
                if (value != _apiKey)
                {
                    _apiKey = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool ConfirmExit
        {
            get => _confirmExit;
            set
            {
                if (value != _confirmExit)
                {
                    _confirmExit = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion Properties
    }
}
