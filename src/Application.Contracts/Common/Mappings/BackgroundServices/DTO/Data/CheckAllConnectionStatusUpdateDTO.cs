namespace Application.Contracts;

public record CheckAllConnectionStatusUpdateDTO()
{
    public int PlexServerId { get; init; }

    public List<int> PlexServerConnectionIds { get; init; } = [];
}
