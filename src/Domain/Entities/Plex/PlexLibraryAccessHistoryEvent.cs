namespace Reaparr.Domain;

public class PlexLibraryAccessHistoryEvent : BaseEntity
{
    public required Guid RefreshRunId { get; init; }

    public required int PlexAccountId { get; init; }

    public string? PlexAccountNameSnapshot { get; init; }

    public int? PlexServerId { get; init; }

    public string? PlexServerNameSnapshot { get; init; }

    public int? PlexLibraryId { get; init; }

    public string? PlexLibraryNameSnapshot { get; init; }

    public required PlexAccessState State { get; init; }

    public required DateTimeOffset OccurredAtUtc { get; init; }

}
