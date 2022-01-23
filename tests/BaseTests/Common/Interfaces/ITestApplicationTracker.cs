using System.Threading.Tasks;

namespace PlexRipper.BaseTests
{
    public interface ITestApplicationTracker
    {
        Task WaitUntilApplicationIsIdle(int checkInterval = 4000, bool logStatus = false);
    }
}