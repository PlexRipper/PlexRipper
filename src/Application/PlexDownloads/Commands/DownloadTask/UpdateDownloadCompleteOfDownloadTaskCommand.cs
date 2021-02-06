using FluentResults;
using MediatR;

namespace PlexRipper.Application.PlexDownloads
{
    public class UpdateDownloadCompleteOfDownloadTaskCommand : IRequest<Result<bool>>
    {
        public UpdateDownloadCompleteOfDownloadTaskCommand(int downloadTaskId, long dataReceived)
        {
            DownloadTaskId = downloadTaskId;
            DataReceived = dataReceived;
        }

        public int DownloadTaskId { get; }

        public long DataReceived { get; }
    }
}