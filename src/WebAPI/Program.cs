using PlexRipper.WebAPI.Common;

namespace PlexRipper.WebAPI;

public class Program
{
    public static void Main()
    {
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
            LogConfig.CloseAndFlush();
        }
    }
}