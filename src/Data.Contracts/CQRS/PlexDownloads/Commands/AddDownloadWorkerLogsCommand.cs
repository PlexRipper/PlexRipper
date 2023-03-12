using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class AddDownloadWorkerLogsCommand : IRequest<Result>
{
    public IList<DownloadWorkerLog> DownloadWorkerLogs { get; }

    public AddDownloadWorkerLogsCommand(IList<DownloadWorkerLog> downloadWorkerLogs)
    {
        DownloadWorkerLogs = downloadWorkerLogs;
    }

    public AddDownloadWorkerLogsCommand(DownloadWorkerLog downloadWorkerLog)
    {
        DownloadWorkerLogs = new List<DownloadWorkerLog>()
        {
            downloadWorkerLog,
        };
    }
}