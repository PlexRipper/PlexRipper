namespace Reaparr.Domain;

public interface IDownloadFileTransferProgress
{
    /// <summary>
    /// Gets or sets the file transfer speeds when the finished download is being merged/moved.
    /// </summary>
    long FileTransferSpeed { get; set; }

    /// <summary>
    /// Gets or sets the total size received of the file in bytes.
    /// </summary>
    long FileDataTransferred { get; set; }

    int CurrentFileTransferPathIndex { get; set; }

    long CurrentFileTransferBytesOffset { get; set; }
}
