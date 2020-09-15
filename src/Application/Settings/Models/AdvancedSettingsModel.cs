using Newtonsoft.Json;
using PlexRipper.Application.Settings.Models.Base;

namespace PlexRipper.Application.Settings.Models
{
    public class AdvancedSettingsModel : BaseModel
    {

        #region Properties

        public DownloadManagerModel DownloadManager { get; set; } = new DownloadManagerModel();

        #endregion Properties

    }
}