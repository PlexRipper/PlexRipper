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

        #endregion
    }
}