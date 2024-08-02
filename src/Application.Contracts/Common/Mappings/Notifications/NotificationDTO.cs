using PlexRipper.Domain;

namespace Application.Contracts;

public class NotificationDTO
{
    public required int Id { get; set; }

    public required NotificationLevel Level { get; set; }

    public required DateTime CreatedAt { get; set; }

    public required string Message { get; set; }

    public required bool Hidden { get; set; }
}
