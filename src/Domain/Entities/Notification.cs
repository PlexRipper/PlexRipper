using System;
using System.ComponentModel.DataAnnotations.Schema;
using FluentResults;

namespace PlexRipper.Domain
{
    public class Notification : BaseEntity
    {
        #region Constructor

        public Notification() { }

        public Notification(IError error)
        {
            Level = NotificationLevel.Error;
            CreatedAt = DateTime.Now;
            Message = error.Message;
        }

        #endregion

        #region Properties

        [Column(Order = 2)]
        public DateTime CreatedAt { get; set; }

        [Column(Order = 3)]
        public bool Hidden { get; set; }

        [Column(Order = 1)]
        public NotificationLevel Level { get; set; }

        [Column(Order = 3)]
        public string Message { get; set; }

        #endregion
    }
}