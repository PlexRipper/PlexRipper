using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class Notification : BaseEntity
{
    #region Constructor

    public Notification() { }

    public Notification(IError error)
    {
        Level = NotificationLevel.Error;
        CreatedAt = DateTime.UtcNow;
        Message = error.Message;
    }

    #endregion

    #region Properties

    [Column(Order = 1)]
    public NotificationLevel Level { get; init; }

    [Column(Order = 2)]
    public DateTime CreatedAt { get; init; }

    [Column(Order = 3)]
    public string Message { get; init; }

    [Column(Order = 4)]
    public bool Hidden { get; init; }

    #endregion
}
