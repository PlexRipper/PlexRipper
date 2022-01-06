using PlexRipper.Application;

namespace PlexRipper.Settings.Models
{
    /// <summary>
    /// Used to model the settings, which is then used to serialize to json.
    /// </summary>
    public class SettingsModel : ISettingsModel
    {
        #region Properties

        public IGeneralSettings GeneralSettings { get; set; }

        public IConfirmationSettings ConfirmationSettings { get; set; }

        public IDateTimeSettings DateTimeSettings { get; set; }

        public IDisplaySettings DisplaySettings { get; set; }

        public IDownloadManagerSettings DownloadManagerSettings { get; set; }

        public ILanguageSettings LanguageSettings { get; set; }

        public IServerSettings ServerSettings { get; set; }

        #endregion
    }
}