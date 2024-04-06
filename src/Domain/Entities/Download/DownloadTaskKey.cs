namespace PlexRipper.Domain;

public record DownloadTaskKey
{
    public required DownloadTaskType Type { get; init; }

    public required Guid Id { get; init; }

    public required int PlexServerId { get; init; }

    public required int PlexLibraryId { get; init; }

    public bool IsValid => Id != Guid.Empty && PlexServerId > 0 && PlexLibraryId > 0 && Type != DownloadTaskType.None;
}