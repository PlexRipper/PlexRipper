namespace Reaparr.BaseTests;

public static partial class FakeData
{
    private static readonly Random _randomInstance = new();

    private static string DownloadFileUrl => "/library/parts/653125/119385313456/file.mp4";

    private static int _uniqueNumber = 1;

    private static int GetUniqueNumber() => Interlocked.Increment(ref _uniqueNumber);
}
