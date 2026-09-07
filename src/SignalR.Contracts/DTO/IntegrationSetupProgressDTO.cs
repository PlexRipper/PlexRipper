namespace Reaparr.SignalR.Contracts;

public record IntegrationSetupProgressDTO
{
    public required Guid IntegrationId { get; init; }
    public required IntegrationSetupProgressStage Stage { get; init; }
    public required bool IsRunning { get; init; }
    public required bool IsSuccess { get; init; }
    public string? Error { get; init; }
}

public enum IntegrationSetupProgressStage
{
    Connecting,
    DownloadClient,
    Indexer,
    Done,
}
