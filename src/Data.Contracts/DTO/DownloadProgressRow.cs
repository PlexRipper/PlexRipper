namespace Reaparr.Data.Contracts;

public record DownloadProgressRow
{
    public required Guid Id { get; init; }

    public required Guid? ParentId { get; init; }

    public required int PlexApiRatingKey { get; init; }

    public required string Title { get; init; }

    public required string FullTitle { get; init; }

    public required PlexMediaType MediaType { get; init; }

    public required DownloadTaskType DownloadTaskType { get; init; }

    public required DownloadStatus DownloadStatus { get; init; }

    public required DateTime CreatedAt { get; init; }

    public required int PlexServerId { get; init; }

    public required int PlexLibraryId { get; init; }

    public required long DataReceived { get; init; }

    public required long DataTotal { get; init; }

    public required decimal Percentage { get; init; }

    public required long DownloadSpeed { get; init; }

    public required int TimeRemaining { get; init; }

    public required long FileTransferSpeed { get; init; }

    public required long FileDataTransferred { get; init; }
}
