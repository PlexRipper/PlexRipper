using System.Collections.Generic;
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public class AddDownloadWorkerLogsCommand : IRequest<Result>
    {
        public IList<DownloadWorkerLog> DownloadWorkerLogs { get; }

        public AddDownloadWorkerLogsCommand(IList<DownloadWorkerLog> downloadWorkerLogs)
        {
            DownloadWorkerLogs = downloadWorkerLogs;
        }
    }
}