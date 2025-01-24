namespace Application.Contracts;

public record SyncServerMediaJobUpdateDTO
{
    public required int PlexServerId { get; init; }

    public required bool ForceSync { get; init; }
}
