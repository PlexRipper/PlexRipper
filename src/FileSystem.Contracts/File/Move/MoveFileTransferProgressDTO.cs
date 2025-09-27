namespace Reaparr.FileSystem.Contracts;

public record MoveFileTransferProgressDTO
{
    /// <summary>
    /// Gets or sets the total size received of the file in bytes.
    /// </summary>
    public required long Transferred { get; init; }

    /// <summary>
    /// Gets or sets the total size of the file in bytes.
    /// </summary>
    public required long DataTotal { get; init; }

    /// <summary>
    /// Gets or sets the file transfer speeds.
    /// </summary>
    public required long FileTransferSpeed { get; init; }
}
