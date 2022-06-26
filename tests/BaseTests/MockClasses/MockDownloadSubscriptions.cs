using PlexRipper.DownloadManager;

namespace PlexRipper.BaseTests;

public class MockDownloadSubscriptions : IDownloadSubscriptions
{
    public Result Setup()
    {
        return Result.Ok();
    }
}