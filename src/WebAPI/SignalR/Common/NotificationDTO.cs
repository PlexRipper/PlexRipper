namespace PlexRipper.WebAPI.SignalR.Common;

public class NotificationDTO
{
    public int Id { get; set; }


    public NotificationLevel Level { get; set; }


    public DateTime CreatedAt { get; set; }


    public string Message { get; set; }


    public bool Hidden { get; set; }
}