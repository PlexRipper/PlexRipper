using PlexRipper.Application.Settings.Models.Base;

namespace PlexRipper.Application.Settings.Models
{
    public class DownloadManagerModel : BaseModel
    {
        #region Fields

        private int _downloadSegments = 4;

        #endregion Fields

        #region Properties

        public virtual int DownloadSegments
        {
            get => _downloadSegments;
            set
            {
                if (value != _downloadSegments)
                {
                    _downloadSegments = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion Properties
    }
}
