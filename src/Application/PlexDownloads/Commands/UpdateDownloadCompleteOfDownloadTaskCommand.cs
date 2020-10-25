using FluentResults;
using MediatR;

namespace PlexRipper.Application.PlexDownloads.Commands
{
    public class UpdateDownloadCompleteOfDownloadTaskCommand : IRequest<Result<bool>>
    {
        public UpdateDownloadCompleteOfDownloadTaskCommand(int downloadTaskId, long dataReceived, long dataTotal)
        {
            DownloadTaskId = downloadTaskId;
            DataReceived = dataReceived;
            DataTotal = dataTotal;
        }

        public int DownloadTaskId { get; }

        public long DataReceived { get; }

        public long DataTotal { get; }
    }
}