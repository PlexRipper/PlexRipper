using Reaparr.SignalR.Contracts;

namespace Reaparr.Application;

/// <summary>
/// Stores the latest buffered progress update for a specific download task.
/// </summary>
public sealed record BufferedProgressUpdate
{
    /// <summary>
    /// Updated node id.
    /// </summary>
    public required Guid NodeId { get; init; }

    /// <summary>
    /// Download task key.
    /// </summary>
    public required DownloadTaskKey Key { get; init; }

    /// <summary>
    /// Latest in-memory progress.
    /// </summary>
    public DownloadTaskProgress? Progress { get; init; }

    /// <summary>
    /// Optional direct-download snapshot used for resume persistence.
    /// </summary>
    public DirectDownloadSnapshot? Snapshot { get; init; }

    /// <summary>
    /// Creates a status-only buffered update placeholder.
    /// </summary>
    public static BufferedProgressUpdate FromStatus(DownloadTaskKey key) => new() { NodeId = key.Id, Key = key };

    /// <summary>
    /// Converts buffered progress into a leaf patch DTO.
    /// </summary>
    public DownloadPatchDTO? ToPatch(Guid parentId, DownloadStatus status)
    {
        if (Progress is null)
            return null;

        return new DownloadPatchDTO
        {
            Id = NodeId,
            ParentId = parentId,
            Status = status,
            Percentage = Progress.Percentage,
            DataReceived = Progress.DataReceived,
            DataTotal = Progress.DataTotal,
            DownloadSpeed = Progress.DownloadSpeed,
            TimeRemaining = Progress.TimeRemaining,
        };
    }
}
