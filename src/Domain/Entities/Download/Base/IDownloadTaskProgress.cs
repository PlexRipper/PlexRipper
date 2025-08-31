namespace Reaparr.Domain;

public interface IDownloadTaskProgress
{
    public long DataTotal { get; }

    public decimal Percentage { get; }

    public long DataReceived { get; }

    public long DownloadSpeed { get; }
}
