using System;
using System.ComponentModel.DataAnnotations.Schema;
using Serilog.Events;

namespace PlexRipper.Domain
{
    public class DownloadWorkerLog : BaseEntity
    {
        #region Properties

        [Column(Order = 1)]
        public DateTime CreatedAt { get; set; }

        [Column(Order = 3)]
        public LogEventLevel LogLevel { get; set; }

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
                case LogEventLevel.Verbose:
                    Logging.Log.Verbose(Message);
                    break;
                case LogEventLevel.Debug:
                    Logging.Log.Debug(Message);
                    break;
                case LogEventLevel.Information:
                    Logging.Log.Information(Message);
                    break;
                case LogEventLevel.Warning:
                    Logging.Log.Warning(Message);
                    break;
                case LogEventLevel.Error:
                    Logging.Log.Error(Message);
                    break;
                case LogEventLevel.Fatal:
                    Logging.Log.Fatal(Message);
                    break;
                default:
                    Logging.Log.Debug(Message);
                    break;
            }
        }

        #endregion
    }
}