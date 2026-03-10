namespace Reaparr.Domain;

public record DownloadTaskProgress : IDownloadTaskProgress
{
    /// <inheritdoc/>
    public long DataTotal { get; set; }

    /// <inheritdoc/>
    public decimal Percentage { get; set; }

    /// <inheritdoc/>
    public long DataReceived { get; set; }

    /// <inheritdoc/>
    public long DownloadSpeed { get; set; }

    /// <inheritdoc/>
    public int TimeRemaining { get; set; }
}
