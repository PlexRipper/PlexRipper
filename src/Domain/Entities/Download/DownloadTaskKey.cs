namespace PlexRipper.Domain;

public record DownloadTaskKey
{
    public DownloadTaskKey() { }

    public DownloadTaskKey(DownloadTaskType Type, Guid Id, int PlexServerId, int PlexLibraryId)
    {
        this.Type = Type;
        this.Id = Id;
        this.PlexServerId = PlexServerId;
        this.PlexLibraryId = PlexLibraryId;
    }

    public DownloadTaskType Type { get; init; }

    public Guid Id { get; init; }

    public int PlexServerId { get; init; }

    public int PlexLibraryId { get; init; }
}