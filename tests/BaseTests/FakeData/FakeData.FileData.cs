namespace PlexRipper.BaseTests
{
    public static partial class FakeData
    {
        public static byte[] GetDownloadFile(int sizeInMb)
        {
            var b = new byte[sizeInMb * 1049000]; // convert mb to byte
            _random.NextBytes(b);
            return b;
        }
    }
}