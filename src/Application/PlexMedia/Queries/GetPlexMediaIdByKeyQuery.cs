namespace PlexRipper.Application
{
    public class GetPlexMediaIdByKeyQuery : IRequest<Result<int>>
    {
        public DownloadTask DownloadTask { get; }

        public GetPlexMediaIdByKeyQuery(DownloadTask downloadTask)
        {
            DownloadTask = downloadTask;
        }
    }
}