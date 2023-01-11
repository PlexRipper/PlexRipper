using ByteSizeLib;

namespace PlexRipper.BaseTests;

public static partial class FakeData
{
    public static byte[] GetDownloadFile(int sizeInMib)
    {
        var b = new byte[(long)ByteSize.FromMebiBytes(sizeInMib).Bytes]; // convert mib to byte
        _random.NextBytes(b);
        return b;
    }
}