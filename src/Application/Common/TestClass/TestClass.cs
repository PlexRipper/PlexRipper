using PlexRipper.Application.Common.Interfaces.Application;
using Serilog;

namespace PlexRipper.Application.Common.TestClass
{
    public class TestClass : ITestClass
    {
        public ILogger Log { get; }

        public TestClass(ILogger log)
        {
            Log = log;
        }

        public void TestLogging()
        {
            Log.Verbose("TestClass.Log => This is a verbose string");
            Log.Debug("TestClass.Log => This is a debug string");
            Log.Warning("TestClass.Log => This is a warning string");
            Log.Information("TestClass.Log => This is an information string");
            Log.Error("TestClass.Log => This is an error string");
            Log.Fatal("TestClass.Log => This is a fatal string");

        }
    }
}
