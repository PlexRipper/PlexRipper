using Logging;
using Xunit.Abstractions;

namespace PlexRipper.BaseTests
{
    public static class BaseDependanciesTest
    {
        public static void SetupLogging(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }
    }
}