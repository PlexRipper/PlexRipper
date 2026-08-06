namespace EntityFrameworkCore.Sqlite.Concurrency.Models;

/// <summary>
/// Represents the WAL checkpoint state returned by
/// <c>PRAGMA wal_checkpoint(PASSIVE)</c>.
/// </summary>
/// <param name="IsBusy">
/// <see langword="true"/> if one or more active readers held a WAL read lock on a
/// frame the checkpoint attempted to overwrite, preventing the checkpoint from
/// completing fully.
/// <para>
/// A persistently <see langword="true"/> value means long-running read transactions are
/// blocking WAL reclamation. As the WAL grows, read performance degrades because readers
/// must scan the growing WAL for every page lookup. Investigate and shorten the longest
/// running read transactions.
/// </para>
/// </param>
/// <param name="TotalWalFrames">
/// The total number of frames currently in the WAL file. Each frame corresponds to one
/// database page (default 4 096 bytes). Multiply by the page size to estimate WAL file
/// size on disk.
/// </param>
/// <param name="CheckpointedFrames">
/// The number of WAL frames successfully transferred back to the main database file
/// during the last checkpoint pass. When <see cref="TotalWalFrames"/> greatly exceeds
/// this value, readers are blocking the checkpoint.
/// </param>
public record WalCheckpointStatus(bool IsBusy, int TotalWalFrames, int CheckpointedFrames)
{
    /// <summary>
    /// Returns <see langword="true"/> when there are WAL frames that have not yet been
    /// checkpointed back into the main database, indicating WAL growth pressure.
    /// </summary>
    public bool HasUncheckpointedFrames => TotalWalFrames > CheckpointedFrames;

    /// <summary>
    /// The percentage of WAL frames that have been successfully checkpointed.
    /// Returns <c>100.0</c> when <see cref="TotalWalFrames"/> is zero.
    /// </summary>
    public double CheckpointProgress =>
        TotalWalFrames == 0 ? 100.0 : (double)CheckpointedFrames / TotalWalFrames * 100.0;
}
