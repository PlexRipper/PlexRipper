namespace PlexRipper.Application;

public class UpdateRootDownloadStatusOfDownloadTaskCommand : IRequest<Result>
{
    public int RootDownloadTaskId { get; }

    public UpdateRootDownloadStatusOfDownloadTaskCommand(int rootDownloadTaskId)
    {
        RootDownloadTaskId = rootDownloadTaskId;
    }
}