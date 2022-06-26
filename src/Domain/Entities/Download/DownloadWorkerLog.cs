using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain
{
    public class DownloadWorkerLog : BaseEntity
    {
        #region Properties

        [Column(Order = 1)]
        public DateTime CreatedAt { get; set; }

        [Column(Order = 3)]
        public NotificationLevel LogLevel { get; set; }

        [Column(Order = 2)]
        public string Message { get; set; }

        #endregion

        #region Relationships

        public DownloadWorkerTask DownloadTask { get; set; }

        public int DownloadWorkerTaskId { get; set; }

        #endregion

        #region Helpers

        public void Log()
        {
            switch (LogLevel)
            {
                case NotificationLevel.Verbose:
                    Logging.Log.Verbose(Message);
                    break;
                case NotificationLevel.Debug:
                    Logging.Log.Debug(Message);
                    break;
                case NotificationLevel.Information:
                    Logging.Log.Information(Message);
                    break;
                case NotificationLevel.Warning:
                    Logging.Log.Warning(Message);
                    break;
                case NotificationLevel.Error:
                    Logging.Log.Error(Message);
                    break;
                case NotificationLevel.Fatal:
                    Logging.Log.Fatal(Message);
                    break;
                case NotificationLevel.None:
                    break;
                case NotificationLevel.Success:
                    break;
                default:
                    Logging.Log.Information(Message);
                    break;
            }
        }

        #endregion
    }
}