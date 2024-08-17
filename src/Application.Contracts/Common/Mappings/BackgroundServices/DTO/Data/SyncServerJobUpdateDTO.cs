namespace Application.Contracts;

public record SyncServerJobUpdateDTO()
{
    public int PlexServerId { get; init; }

    public bool ForceSync { get; init; }
}
