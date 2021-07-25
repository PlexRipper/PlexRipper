using System;
using System.ComponentModel.DataAnnotations.Schema;
using FluentResults;

namespace PlexRipper.Domain
{
    public class Notification : BaseEntity
    {
        [Column(Order = 1)]
        public string Level { get; set; }

        [Column(Order = 2)]
        public DateTime CreatedAt { get; set; }

        [Column(Order = 3)]
        public string Message { get; set; }

        [Column(Order = 3)]
        public bool Hidden { get; set; }

        public Notification() { }

        public Notification(Error error)
        {
            NotificationLevel = NotificationLevel.Error;
            CreatedAt = DateTime.Now;
            Message = error.Message;
        }

        [NotMapped]
        public NotificationLevel NotificationLevel
        {
            get
            {
                return Level switch
                {
                    "none" => NotificationLevel.None,
                    "info" => NotificationLevel.Info,
                    "success" => NotificationLevel.Success,
                    "warning" => NotificationLevel.Warning,
                    "error" => NotificationLevel.Error,
                    _ => NotificationLevel.None,
                };
            }

            set
            {
                switch (value)
                {
                    case NotificationLevel.None:
                        Level = "none";
                        return;
                    case NotificationLevel.Info:
                        Level = "info";
                        return;
                    case NotificationLevel.Success:
                        Level = "success";
                        return;
                    case NotificationLevel.Warning:
                        Level = "warning";
                        return;
                    case NotificationLevel.Error:
                        Level = "error";
                        return;
                }
            }
        }
    }
}