using FluentResults;
using Logging;
using PlexRipper.WebAPI.Common;
using Serilog.Events;

namespace PlexRipper.WebAPI
{
    public class Program
    {
        public static void Main()
        {
            Log.SetupLogging(LogEventLevel.Verbose);

            try
            {
                PlexRipperHost.Setup().Build().Run();
            }
            catch (Exception e)
            {
                Result.Fail(new ExceptionalError(e)).LogFatal();
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                Log.CloseAndFlush();
            }
        }
    }
}