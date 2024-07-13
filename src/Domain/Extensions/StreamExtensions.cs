namespace PlexRipper.Domain;

public static class StreamExtensions
{
    public static FileStream? WaitForFile(string fullPath, FileMode mode, FileAccess access, FileShare share)
    {
        FileStream? fs = null;
        for (var numTries = 0; numTries < 10; numTries++)
        {
            try
            {
                fs = new FileStream(fullPath, mode, access, share);
                return fs;
            }
            catch (IOException)
            {
                if (fs != null)
                    fs.Dispose();

                Thread.Sleep(50);
            }
        }

        return null;
    }
}
