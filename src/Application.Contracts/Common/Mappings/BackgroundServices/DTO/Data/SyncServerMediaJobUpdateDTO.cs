namespace Application.Contracts;

public record SyncServerMediaJobUpdateDTO()
{
    public int PlexServerId { get; init; }

    public bool ForceSync { get; init; }
}
