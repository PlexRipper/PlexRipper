using System.IO;
using System.Threading;

namespace PlexRipper.Domain
{
    public static class StreamExtensions
    {
        public static FileStream WaitForFile(string fullPath, FileMode mode, FileAccess access, FileShare share)
        {
            for (int numTries = 0; numTries < 10; numTries++)
            {
                FileStream fs = null;
                try
                {
                    fs = new FileStream(fullPath, mode, access, share);
                    return fs;
                }
                catch (IOException)
                {
                    if (fs != null)
                    {
                        fs.Dispose();
                    }

                    Thread.Sleep(50);
                }
            }

            return null;
        }
    }
}