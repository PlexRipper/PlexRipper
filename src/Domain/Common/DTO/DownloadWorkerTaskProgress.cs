namespace Reaparr.Domain;

public record DownloadWorkerTaskProgress : IDownloadTaskProgress
{
    public required int Id { get; init; }

    public required long DataTotal { get; init; }

    public required decimal Percentage { get; init; }

    public required long DataReceived { get; init; }

    public required long ElapsedTime { get; init; }

    public required DownloadStatus Status { get; init; }

    public required long DownloadSpeed { get; init; }
}
