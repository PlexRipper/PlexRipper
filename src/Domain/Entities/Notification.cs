using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class Notification : BaseEntity
{
    public Notification() { }

    public Notification(IError error)
    {
        Level = NotificationLevel.Error;
        CreatedAt = DateTime.UtcNow;
        Message = error.Message;
    }

    [Column(Order = 1)]
    public NotificationLevel Level { get; init; }

    [Column(Order = 2)]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    [Column(Order = 3)]
    public string Message { get; init; } = string.Empty;

    [Column(Order = 4)]
    public bool Hidden { get; init; }
}
