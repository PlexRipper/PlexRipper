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
                    Domain.Log.Verbose(Message);
                    break;
                case LogEventLevel.Debug:
                    Domain.Log.Debug(Message);
                    break;
                case LogEventLevel.Information:
                    Domain.Log.Information(Message);
                    break;
                case LogEventLevel.Warning:
                    Domain.Log.Warning(Message);
                    break;
                case LogEventLevel.Error:
                    Domain.Log.Error(Message);
                    break;
                case LogEventLevel.Fatal:
                    Domain.Log.Fatal(Message);
                    break;
                default:
                    Domain.Log.Debug(Message);
                    break;
            }
        }

        #endregion
    }
}