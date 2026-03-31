namespace Reaparr.SignalR.Contracts;

public record DownloadPatchDTO
{
    public required Guid Id { get; init; }

    public required Guid ParentId { get; init; }

    public required DownloadStatus Status { get; init; }

    public required decimal Percentage { get; init; }

    public required long DataReceived { get; init; }

    public required long DataTotal { get; init; }

    public required long DownloadSpeed { get; init; }

    public required int TimeRemaining { get; init; }
}
