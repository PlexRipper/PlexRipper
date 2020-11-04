using System;
using System.ComponentModel.DataAnnotations.Schema;

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

        [NotMapped]
        public NotificationLevel NotificationLevel
        {
            get
            {
                return Level switch
                {
                    "none" => NotificationLevel.None,
                    "debug" => NotificationLevel.Debug,
                    "info" => NotificationLevel.Info,
                    "success" => NotificationLevel.Success,
                    "warning" => NotificationLevel.Warning,
                    "error" => NotificationLevel.Error,
                };
            }

            set
            {
                switch (value)
                {
                    case NotificationLevel.None:
                        Level = "none";
                        return;
                    case NotificationLevel.Debug:
                        Level = "debug";
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