namespace Reaparr.SignalR.Contracts;

public record LibraryComparisonCompletedDTO
{
    public required IReadOnlyList<int> AffectedLibraryIds { get; init; }

    public required PlexMediaType MediaType { get; init; }

    public required DateTime CompletedAt { get; init; }
}
