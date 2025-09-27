namespace Reaparr.Domain;

public record DownloadFileTransferProgress : IDownloadFileTransferProgress
{
    public required long FileTransferSpeed { get; set; }

    public required long FileDataTransferred { get; set; }

    public required long CurrentFileTransferBytesOffset { get; set; }
}
