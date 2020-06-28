using PlexRipper.Application.Common.Interfaces.Application;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common.TestClass
{
    public class TestClass : ITestClass
    {

        public TestClass()
        {

        }

        public void TestLogging()
        {
            Log.Verbose("This is a verbose string");
            Log.Debug("This is a debug string");
            Log.Warning("This is a warning string");
            Log.Information("This is an information string");
            Log.Error("This is an error string");
            Log.Fatal("This is a fatal string");
        }
    }
}
